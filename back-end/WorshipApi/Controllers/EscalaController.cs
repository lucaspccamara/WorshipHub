using Microsoft.AspNetCore.Mvc;

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
    }
}
