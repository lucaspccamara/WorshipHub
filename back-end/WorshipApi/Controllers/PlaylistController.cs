using Microsoft.AspNetCore.Mvc;

namespace WorshipApi.Controllers
{
    [Route("api/playlists")]
    public class PlaylistController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<List<string>> GetPlaylist()
        {
            var playlist = new List<string>() 
            {
                "música 1", "música 2", "música 3", "música 4"
            };

            return Ok(playlist);
        }
    }
}
