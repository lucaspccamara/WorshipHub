using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorshipApplication.Services;
using WorshipDomain.DTO.HomePage;

namespace WorshipApi.Controllers
{
    [Route("api/homes")]
    public class HomeController : ControllerBase
    {
        [HttpPost("calendar")]
        public ActionResult<HomeCalendarDto> GetCalendar(
            [FromServices] HomeService _homeService,
            [FromBody] HomeCalendarFilterDto request)
        {
            try
            {
                var user = HttpContext.User;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = _homeService.GetCalendar(request, int.Parse(userId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
