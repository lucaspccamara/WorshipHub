using Microsoft.AspNetCore.Mvc;
using WorshipApi.Core;
using WorshipApplication.Services;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Music;
using WorshipDomain.Enums;

namespace WorshipApi.Controllers
{
    [Route("api/musics")]
    public class MusicController : ControllerBase
    {
        [HttpPost("list")]
        public ActionResult<ResultFilter<MusicOverviewDTO>> GetMusics(
            [FromServices] MusicService _musicService,
            [FromBody] ApiRequest<MusicFilterDTO> request)
        {
            var result = _musicService.GetListPaged(request);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public ActionResult<MusicDTO> GetMusic(
            [FromServices] MusicService _musicService,
            [FromRoute] int id)
        {
            return _musicService.GetMusic(id);
        }

        [HttpPost]
        [AuthorizeRoles(Role.Admin, Role.Leader, Role.Minister)]
        public ActionResult CreateMusic(
            [FromServices] MusicService _musicService,
            [FromBody] MusicCreationDTO musicCreationDTO)
        {
            return _musicService.Create(musicCreationDTO);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles(Role.Admin, Role.Leader, Role.Minister)]
        public ActionResult UpdateMusic(
            [FromServices] MusicService _musicService,
            [FromRoute] int id,
            [FromBody] MusicDTO musicDTO)
        {
            if (id != musicDTO.Id)
                return BadRequest("Id da requisição não corresponde ao Id da entidade.");

            _musicService.Update(musicDTO);
            return NoContent();
        }
    }
}
