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
        [AuthorizeRoles(Role.Admin, Role.Leader, Role.Minister)]
        public ActionResult<ResultFilter<ScheduleOverviewDTO>> GetSchedules(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] ApiRequest<ScheduleFilterDTO> request)
        {
            var user = HttpContext.User;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            if (role == Role.Minister.ToString())
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userId, out int parsedUserId))
                {
                    request.Filters.MinisterId = parsedUserId;
                }
            }

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
        [AuthorizeRoles(Role.Admin, Role.Leader)]
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
        [AuthorizeRoles(Role.Admin, Role.Leader, Role.Minister)]
        public async Task<ActionResult> Transition(
            [FromServices] ScheduleService _scheduleService,
            [FromBody] TransitionDto dto)
        {
            if (dto == null || dto.ScheduleIds == null || dto.ScheduleIds.Count == 0) return BadRequest();
            try
            {
                await _scheduleService.TransitionSchedulesAsync(dto.ScheduleIds, dto.NewStatus);

                if (dto.NewStatus == (int)ScheduleStatus.CollectingAvailability)
                {
                    await _scheduleService.CollectingAvailabilitiesTransitionAsync(dto.ScheduleIds, dto.NewStatus);
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
        [AuthorizeRoles(Role.Admin, Role.Leader, Role.Minister)]
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
            [FromBody] IEnumerable<ScheduleTrackInputDto> tracks,
            int id)
        {
            try
            {
                _scheduleService.SaveScheduleRepertoire(id, tracks ?? Enumerable.Empty<ScheduleTrackInputDto>());
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("assignments/details")]
        [AuthorizeRoles(Role.Admin, Role.Leader, Role.Minister)]
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
        [AuthorizeRoles(Role.Admin, Role.Leader)]
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

        [HttpPost("{id}/notify")]
        [AuthorizeRoles(Role.Admin, Role.Leader)]
        public async Task<ActionResult> NotifyUpdate(
            [FromServices] ScheduleService _scheduleService,
            int id)
        {
            try
            {
                await _scheduleService.NotifyScheduleUpdateAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
