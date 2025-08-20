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
        public ScheduleService(IScheduleRepository repository) : base(repository)
        {
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
    }
}
