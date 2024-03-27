using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorshipApi.Controllers
{
    [Route("api/[Controller]")]
    [Authorize]
    public class RepertorioController : ControllerBase
    {
        [HttpGet()]
        public ActionResult<List<string>> GetRepertorio()
        {
            var repertorio = new List<string>() 
            {
                "música 1", "música 2", "música 3", "música 4"
            };

            return Ok(repertorio);
        }
    }
}
