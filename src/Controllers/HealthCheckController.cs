using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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