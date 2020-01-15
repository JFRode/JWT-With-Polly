using APIClient.Clients;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace APIClient.Controllers
{
    [Route("{controller}")]
    public class DoSomethingController : Controller
    {
        private readonly IAPIWhoSayNiClient _APIWhoSayNiClient;

        public DoSomethingController(IAPIWhoSayNiClient apiWhoSayNiClient)
        {
            _APIWhoSayNiClient = apiWhoSayNiClient;
        }

        [HttpGet]
        public async Task<IActionResult> DoSomething(CancellationToken cancellationToken)
        {
            var response = await _APIWhoSayNiClient.Get(cancellationToken);
            return Ok(response);
        }
    }
}