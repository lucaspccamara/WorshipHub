using Microsoft.AspNetCore.Mvc;
using WorshipApi.Core;
using WorshipApplication.Services;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.DTO.User;
using WorshipDomain.Enums;

namespace WorshipApi.Controllers
{
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        [HttpPost("list")]
        [AuthorizeRoles(Role.Admin, Role.Leader)]
        public ActionResult<ResultFilter<UserOverviewDTO>> GetUsers(
            [FromServices] UserService _userService,
            [FromBody] ApiRequest<UserFilterDTO> request)
        {
            var result = _userService.GetListPaged(request);

            return Ok(result);
        }

        [HttpGet("profile/{id}")]
        public ActionResult<UserProfileDTO> GetUserProfile(
            [FromServices] UserService _userService,
            [FromRoute] int id)
        {
            return _userService.GetUserProfile(id);
        }

        [HttpPost]
        [AuthorizeRoles(Role.Admin, Role.Leader)]
        public ActionResult CreateUser(
            [FromServices] UserService _userService,
            [FromBody] UserCreationDTO userCreationDTO)
        {
            return _userService.Create(userCreationDTO);
        }

        [HttpPut("profile/{id}")]
        public ActionResult UpdateUser(
            [FromServices] UserService _userService,
            [FromRoute] int id,
            [FromBody] UserProfileDTO userProfileDTO)
        {
            if (id != userProfileDTO.Id)
                return BadRequest("Id da requisição não corresponde ao Id da entidade.");

            _userService.UpdateProfile(userProfileDTO);
            return NoContent();
        }
    }
}
