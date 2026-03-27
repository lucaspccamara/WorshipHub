using FluentResults;
using Microsoft.AspNetCore.Mvc;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.Entities;
using WorshipDomain.Enums;
using WorshipDomain.Repository;


namespace WorshipApplication.Services
{
    public class ScheduleService : ServiceBase<int, Schedule, IScheduleRepository>
    {
        private readonly IUserRepository _userRepo;
        private readonly IMusicRepository _musicRepo;
        private readonly IScheduleAvailabilitiesRepository _scheduleAvailabilitiesRepository;
        private readonly FcmNotificationService _fcmNotificationService;

        public ScheduleService(
            IScheduleRepository repository,
            IUserRepository userRepo,
            IMusicRepository musicRepo,
            IScheduleAvailabilitiesRepository scheduleAvailabilitiesRepository,
            FcmNotificationService fcmNotificationService) : base(repository)
        {
            _userRepo = userRepo;
            _musicRepo = musicRepo;
            _scheduleAvailabilitiesRepository = scheduleAvailabilitiesRepository;
            _fcmNotificationService = fcmNotificationService;
        }

        public Result<string> CreateSchedule(IEnumerable<ScheduleCreationDTO> schedulesCreationDTO)
        {
            int registered = 0;
            List<string> errorMessage = new List<string>();

            foreach (var scheduleCreationDTO in schedulesCreationDTO)
            {
                if (!DateTime.TryParse(scheduleCreationDTO.Date, out DateTime date))
                {
                    errorMessage.Add($"Data inválida: {scheduleCreationDTO.Date}");
                    continue;
                }

                if (_repository.ExistSchedule(date, scheduleCreationDTO.EventType))
                {
                    errorMessage.Add($"Já existe uma escala para a data {date.ToShortDateString()} e evento {scheduleCreationDTO.EventType}.");
                    continue;
                }

                var schedule = new Schedule
                {
                    Date = date,
                    EventType = scheduleCreationDTO.EventType,
                    Status = ScheduleStatus.Created
                };

                try
                {
                    _repository.Insert(schedule);
                    registered++;
                }
                catch (Exception ex)
                {
                    errorMessage.Add($"Erro ao inserir a escala: {ex.Message}");
                }
            }

            string resultMessage = registered > 0
                ? $"{registered} escalas criadas com sucesso."
                : "Nenhuma escala foi cadastrada.";

            if (errorMessage.Count > 0)
            {
                resultMessage += "\nErros: " + string.Join(", ", errorMessage);
            }

            return Result.Ok(resultMessage);
        }

        public ActionResult UpdateSchedule(ScheduleDTO scheduleDTO)
        {
            DateTime.TryParse(scheduleDTO.Date, out DateTime date);

            var existSchedule = _repository.GetList().Where(s => s.Id != scheduleDTO.Id && s.Date == date && s.EventType == scheduleDTO.EventType).FirstOrDefault();

            if (existSchedule != null)
                return new ConflictResult();

            Schedule schedule = new()
            {
                Id = scheduleDTO.Id,
                Date = date,
                EventType = scheduleDTO.EventType,
                Status = scheduleDTO.Status
            };

            _repository.Update(schedule);
            return new NoContentResult();
        }

        public ResultFilter<ScheduleOverviewDTO> GetListPaged(ApiRequest<ScheduleFilterDTO> request)
        {
            return _repository.GetListPaged(request);
        }

        public async Task TransitionSchedulesAsync(IEnumerable<int> scheduleIds, int newStatus)
        {
            if (scheduleIds == null) throw new ArgumentNullException(nameof(scheduleIds));
            
            _repository.UpdateStatus(scheduleIds, newStatus);

            // Se a transição for para "Aguardando Repertório", notificar ministros
            if (newStatus == (int)ScheduleStatus.WaitingRepertoire)
            {
                foreach (var id in scheduleIds)
                {
                    await NotifyWaitingRepertoireAsync(id);
                }
            }
            // Se a transição for para "Concluído", notificar músicos (exceto ministros)
            else if (newStatus == (int)ScheduleStatus.Completed)
            {
                foreach (var id in scheduleIds)
                {
                    await NotifyRepertoireReleasedAsync(id);
                }
            }
        }

        public List<ScheduleAvailabilityDTO> GetPendingAvailabilities(int userId)
        {
            return _scheduleAvailabilitiesRepository.GetPendingByUser(userId);
        }

        public void RespondAvailability(int availabilityId, bool available, int userId)
        {
            // busca registro e valida propriedade
            var record = _scheduleAvailabilitiesRepository.GetById(availabilityId);
            if (record == null) throw new InvalidOperationException("Registro não encontrado.");
            if (record.UserId != userId) throw new UnauthorizedAccessException("Registro não pertence ao usuário.");

            // valida se a escala ainda está em status de coleta
            var schedule = _repository.Get(record.ScheduleId);
            if (schedule == null) throw new InvalidOperationException("Escala não encontrada.");
            if (schedule.Status != ScheduleStatus.CollectingAvailability)
                throw new InvalidOperationException("Não é possível alterar resposta: escala não está em coleta.");

            // atualiza
            _scheduleAvailabilitiesRepository.UpdateAvailability(availabilityId, available);
        }

        public async Task CollectingAvailabilitiesTransitionAsync(IEnumerable<int> scheduleIds, int newStatus)
        {
            var users = _repository.GetUsersToNotifyForTransition(scheduleIds, newStatus);

            foreach (var scheduleId in scheduleIds)
            {
                users.ForEach(user => 
                {
                    _scheduleAvailabilitiesRepository.Insert(new ScheduleAvailabilities { ScheduleId = scheduleId, UserId = user.Id });
                });
            }

            // Notifica cada usuário apenas uma vez, independentemente de quantas escalas foram liberadas
            var tasks = users
                .Where(u => !string.IsNullOrWhiteSpace(u.FcmToken))
                .Select(u => _fcmNotificationService.SendNotificationAsync(
                    u.FcmToken,
                    "Novas escalas disponíveis! 🗓️",
                    $"Olá {u.Name}, existem escalas que precisam da sua atenção. Informe sua disponibilidade no app.",
                    "/availabilities"
                )).ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task NotifyWaitingRepertoireAsync(int scheduleId)
        {
            var schedule = _repository.Get(scheduleId);
            if (schedule == null) return;

            var users = _repository.GetAssignedUsers(scheduleId);
            
            // Notifica apenas os Ministros (Position 0)
            var tasks = users
                .Where(u => u.Position == 0 && !string.IsNullOrWhiteSpace(u.FcmToken))
                .Select(u => _fcmNotificationService.SendNotificationAsync(
                    u.FcmToken,
                    "Escolha do Repertório 🎸",
                    $"A escala do dia {schedule.Date:dd/MM} está aguardando a sua escolha de músicas.",
                    $"/schedules" // Redirecionar para escalas ou tela específica de repertório
                )).ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task NotifyRepertoireReleasedAsync(int scheduleId)
        {
            var schedule = _repository.Get(scheduleId);
            if (schedule == null) return;

            var users = _repository.GetAssignedUsers(scheduleId);
            
            // Notifica todos os membros EXCETO o Ministro (Position 0)
            var tasks = users
                .Where(u => u.Position != 0 && !string.IsNullOrWhiteSpace(u.FcmToken))
                .Select(u => _fcmNotificationService.SendNotificationAsync(
                    u.FcmToken,
                    "Repertório Liberado! ✨",
                    $"O repertório para a escala do dia {schedule.Date:dd/MM} já está disponível. Confira no app!",
                    $"/"
                )).ToArray();

            await Task.WhenAll(tasks);
        }

        public ScheduleRepertoireDto GetScheduleRepertoireDetails(int scheduleId)
        {
            return _repository.GetScheduleRepertoireDetails(scheduleId);
        }

        public void SaveScheduleRepertoire(int scheduleId, IEnumerable<ScheduleTrackInputDto> tracks)
        {
            _repository.SaveScheduleRepertoire(scheduleId, tracks);
        }

        public SchedulesAssignmentsDetailsDto? GetSchedulesAssignmentsDetails(IEnumerable<int> scheduleIds)
        {
            if (scheduleIds == null) throw new ArgumentNullException(nameof(scheduleIds));
            return _repository.GetSchedulesAssignmentsDetails(scheduleIds);
        }

        public void SaveAssignments(int scheduleId, ScheduleAssignmentsDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            // repository expects position -> list of userIds mapping
            _repository.SaveAssignments(scheduleId, dto.Assignments);
        }

        public async Task NotifyScheduleUpdateAsync(int scheduleId)
        {
            var schedule = _repository.Get(scheduleId);
            if (schedule == null) return;

            var users = _repository.GetAssignedUsers(scheduleId);

            // Notifica todos os membros na escala (incluindo o Ministro)
            var tasks = users
                .Where(u => !string.IsNullOrWhiteSpace(u.FcmToken))
                .Select(u => _fcmNotificationService.SendNotificationAsync(
                    u.FcmToken,
                    "Escala Atualizada! 🗓️",
                    $"Houve uma alteração na escala do dia {schedule.Date:dd/MM}. Confira no app!",
                    $"/"
                )).ToArray();

            await Task.WhenAll(tasks);
        }

        public async Task NotifySchedulesUpdateAsync(IEnumerable<int> scheduleIds)
        {
            if (scheduleIds == null) return;
            foreach (var id in scheduleIds)
            {
                await NotifyScheduleUpdateAsync(id);
            }
        }
    }
}
