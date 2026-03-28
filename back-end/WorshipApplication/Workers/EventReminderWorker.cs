using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorshipApplication.Services;
using WorshipDomain.Repository;

namespace WorshipApplication.Workers
{
    /// <summary>
    /// BackgroundService que verifica a cada hora se existem escalas concluídas
    /// que ocorrerão em 2 dias e envia push notifications para os membros escalados
    /// cujo horário local seja >= 12:00 e que ainda não receberam o lembrete.
    /// </summary>
    public class EventReminderWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventReminderWorker> _logger;

        // Verifica a cada 1 hora
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        public EventReminderWorker(IServiceScopeFactory scopeFactory, ILogger<EventReminderWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventReminderWorker iniciado.");

            // Aguarda a inicialização completa do banco de dados em ambientes Docker
            _logger.LogInformation("EventReminderWorker aguardando 30s antes da primeira verificação...");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            using var timer = new PeriodicTimer(CheckInterval);

            // Executa imediatamente na inicialização e depois a cada CheckInterval
            do
            {
                try
                {
                    await SendRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar lembretes de escalas.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task SendRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IScheduleRepository>();
            var fcmService = scope.ServiceProvider.GetRequiredService<FcmNotificationService>();

            // Escalas concluídas com data daqui a 2 dias
            _logger.LogInformation("EventReminderWorker verificando escalas em d+2...");
            var schedules = scheduleRepo.GetSchedulesStartingIn(2).ToList();

            if (schedules.Count == 0)
            {
                _logger.LogInformation("Nenhuma escala encontrada para lembrete em d+2.");
                return;
            }

            _logger.LogInformation("{Count} escala(s) encontrada(s) para lembrete.", schedules.Count);

            foreach (var schedule in schedules)
            {
                if (stoppingToken.IsCancellationRequested) break;

                // Apenas membros que ainda não receberam o lembrete
                var pendingUsers = scheduleRepo.GetPendingReminderUsers(schedule.Id);

                foreach (var user in pendingUsers)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    // Converter hora atual UTC para o timezone do usuário
                    TimeZoneInfo userTz;
                    try
                    {
                        userTz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                    }
                    catch
                    {
                        _logger.LogWarning(
                            "Timezone inválido '{Tz}' para o usuário {Id}. Usando 'America/Sao_Paulo'.",
                            user.Timezone, user.Id);
                        userTz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                    }

                    var userLocalNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTz);
                    var userToday = userLocalNow.Date;

                    // Valida que a escala é EXATAMENTE daqui a 2 dias no fuso do usuário
                    var daysUntil = (schedule.Date.Date - userToday).Days;
                    if (daysUntil != 2)
                        continue;

                    // Só envia se for >= 12:00 no horário do membro
                    if (userLocalNow.Hour < 12)
                        continue;

                    _logger.LogInformation(
                        "Enviando lembrete para usuário {UserId} (escala {ScheduleId}) — local hour: {Hour}h, tz: {Tz}.",
                        user.Id, schedule.Id, userLocalNow.Hour, user.Timezone);

                    if (!string.IsNullOrWhiteSpace(user.FcmToken))
                    {
                        await fcmService.SendNotificationAsync(
                            user.FcmToken,
                            "Lembrete de Escala 🎵",
                            $"Você tem uma escala em {schedule.Date:dd/MM}. Confira o repertório no app!",
                            "/"
                        );
                    }

                    scheduleRepo.MarkUserReminderAsSent(schedule.Id, user.Id);
                }
            }
        }

    }
}
