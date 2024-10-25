using Microsoft.AspNetCore.Mvc;
using WorshipApplication.Services;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Escala;

namespace WorshipApi.Controllers
{
    [Route("api/escalas")]
    public class EscalaController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<ResultFilter<EscalaOverviewDTO>> GetEscala(
            [FromServices] EscalaService escalaService,
            ApiRequest<EscalaFilterDTO> request)
        {
            var repertorio = new List<string>()
            {
                "música 1", "música 2", "música 3", "música 4"
            };

            return Ok(repertorio);
        }

        [HttpPost()]
        public ActionResult CreateEscala(
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
