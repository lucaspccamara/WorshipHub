using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        public ActionResult<ResultFilter<ScheduleOverviewDTO>> GetSchedules(
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

        [HttpPut("{id}")]
        [AuthorizeRoles(Role.Admin, Role.Leader)]
        public ActionResult UpdateSchedule(
            [FromServices] ScheduleService _scheduleService,
            [FromRoute] int id,
            [FromBody] ScheduleDTO scheduleDTO)
        {
            if (id != scheduleDTO.Id)
                return BadRequest("Id da requisição não corresponde ao Id da entidade.");

            return _scheduleService.UpdateSchedule(scheduleDTO);
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

        [HttpPost("transition")]
        public ActionResult Transition(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] TransitionDto dto)
        {
            if (dto == null || dto.ScheduleIds == null || dto.ScheduleIds.Count == 0) return BadRequest();
            try
            {
                _scheduleService.TransitionSchedules(dto.ScheduleIds, dto.NewStatus);

                if (dto.NewStatus == ScheduleStatus.CollectingAvailability.GetHashCode())
                {
                    // fire-and-forget notifications
                    _ = Task.Run(async () =>
                    {
                        try { await _scheduleService.CollectingAvailabilitiesTransitionAsync(dto.ScheduleIds, dto.NewStatus); }
                        catch { /* log if needed */ }
                    });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("availabilities/pending")]
        public ActionResult GetPendindAvailabilities(
            [FromServices] ScheduleService _scheduleService)
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();

                var items = _scheduleService.GetPendingAvailabilities(int.Parse(userId));
                return Ok(new { data = items });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("availabilities/respond")]
        public ActionResult RespondAvailability(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] ScheduleAvailabilityResponseDTO dto)
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Unauthorized();

                _scheduleService.RespondAvailability(dto.Id, dto.Available, int.Parse(userId));
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}/repertoire")]
        [AuthorizeRoles(Role.Admin, Role.Leader)]
        public ActionResult GetRepertoire(
            [FromServices] ScheduleService _scheduleService,
            int id)
        {
            try
            {
                var dto = _scheduleService.GetScheduleRepertoireDetails(id);
                if (dto == null) return NotFound();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/repertoire")]
        [AuthorizeRoles(Role.Admin, Role.Leader, Role.Minister)]
        public ActionResult SaveRepertoire(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] int[] musicIds,
            int id)
        {
            try
            {
                _scheduleService.SaveScheduleRepertoire(id, musicIds ?? Array.Empty<int>());
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("assignments/details")]
        [AuthorizeRoles(Role.Admin, Role.Leader)]
        public ActionResult GetSchedulesAssignmentsDetails(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] int[] scheduleIds)
        {
            if (scheduleIds == null || scheduleIds.Length == 0) return BadRequest("scheduleIds required");
            try
            {
                var dto = _scheduleService.GetSchedulesAssignmentsDetails(scheduleIds);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/assignments")]
        public ActionResult SaveAssignments(
            [FromServices] ScheduleService _scheduleService,
            int id,
            [FromBody] ScheduleAssignmentsDto dto)
        {
            if (dto == null) return BadRequest();
            try
            {
                _scheduleService.SaveAssignments(id, dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
