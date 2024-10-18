using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkerBrokerInteration.Controller
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        [AllowAnonymous]
        [Route("/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
