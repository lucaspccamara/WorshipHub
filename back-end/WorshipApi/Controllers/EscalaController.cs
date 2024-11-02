using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorshipApi.Core;
using WorshipApplication.Services;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Escala;
using WorshipDomain.Enums;

namespace WorshipApi.Controllers
{
    [Route("api/escalas")]
    public class EscalaController : ControllerBase
    {
        [HttpPost("list")]
        public ActionResult<ResultFilter<EscalaOverviewDTO>> GetEscala(
            [FromServices] EscalaService _escalaService,
            ApiRequest<EscalaFilterDTO> request)
        {
            var result = _escalaService.GetListPaged(request);

            return Ok(result);
        }

        [HttpPost()]
        [AuthorizeRoles(Perfil.Admin, Perfil.Lider)]
        public ActionResult CreateEscala(
            [FromServices] EscalaService _escalaService,
            [FromBody] IEnumerable<EscalaCreationDTO> escalasCreationDTO)
        {
            var result = _escalaService.CreateEscala(escalasCreationDTO);
            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(
            [FromServices] EscalaService _escalaService,
            int id)
        {
            var existingEscala = _escalaService.Get(id);
            if (existingEscala == null)
            {
                return NotFound();
            }

            _escalaService.Delete(id);
            return NoContent();
        }
    }
}
