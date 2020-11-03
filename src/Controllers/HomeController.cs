using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using Brighid.Identity.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Brighid.Identity.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        private object? GetJsonValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    return element.GetInt64();
                case JsonValueKind.Array:
                    return element.EnumerateArray().Select(GetJsonValue);
                case JsonValueKind.Object:
                    return element.EnumerateObject().ToDictionary((prop) => prop.Name, (prop) => GetJsonValue(prop.Value));
                default:
                    return null;
            }
        }

        [HttpGet("/internal")]
        public IActionResult GetToken()
        {
            return NoContent();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
