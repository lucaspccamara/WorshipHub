using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorshipApi.Core;
using WorshipApplication.Services;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.Enums;

namespace WorshipApi.Controllers
{
    [Route("api/schedules")]
    public class ScheduleController : ControllerBase
    {
        [HttpPost("list")]
        public ActionResult<ResultFilter<ScheduleOverviewDTO>> GetSchedule(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] ApiRequest<ScheduleFilterDTO> request)
        {
            var result = _scheduleService.GetListPaged(request);

            return Ok(result);
        }

        [HttpPost()]
        [AuthorizeRoles(Role.Admin, Role.Leader)]
        public ActionResult CreateSchedule(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] IEnumerable<ScheduleCreationDTO> scheduleCreationDTO)
        {
            var result = _scheduleService.CreateSchedule(scheduleCreationDTO);
            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(
            [FromServices] ScheduleService _scheduleService,
            int id)
        {
            var schedule = _scheduleService.Get(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _scheduleService.Delete(id);
            return NoContent();
        }
    }
}
