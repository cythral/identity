
using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity.Controllers
{
    [ApiController]
    [Route("/healthcheck")]
    public class HealthCheckController : ControllerBase
    {

        public HealthCheckController()
        {

        }

        [HttpGet]
        public ActionResult<string> GetContainerHealth()
        {
            return Ok("OK");
        }
    }
}
