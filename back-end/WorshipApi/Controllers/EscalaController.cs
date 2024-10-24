using Microsoft.AspNetCore.Mvc;
using WorshipApplication.DTO.Escala;
using WorshipApplication.Services;

namespace WorshipApi.Controllers
{
    [Route("api/escalas")]
    public class EscalaController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<List<string>> GetEscala()
        {
            var repertorio = new List<string>()
            {
                "música 1", "música 2", "música 3", "música 4"
            };

            return Ok(repertorio);
        }

        [HttpPost()]
        public ActionResult PostEscala(
            [FromServices] EscalaService _escalaService,
            [FromBody] IEnumerable<EscalaCreationDTO> escalasCreationDTO)
        {
            var result = _escalaService.CreateEscala(escalasCreationDTO);
            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }
    }
}
