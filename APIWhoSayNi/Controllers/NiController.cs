using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIWhoSayNi.Controllers
{
    [Route("api")]
    [Authorize()]
    public class NiController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Ni!");
        }
    }
}