using System.Threading.Tasks;

using Brighid.Identity.Sns;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brighid.Identity
{
    [Route("/mock")]
    public class MockController : Controller
    {
        private readonly MockControllerCalls calls;

        public MockController(
            MockControllerCalls calls
        )
        {
            this.calls = calls;
        }

        [HttpPut]
        [AllowAnonymous]
        public async Task<ActionResult> MockAction([FromBody] CloudFormationResponse response)
        {
            await Task.CompletedTask;
            calls.Add(response);
            return Ok();
        }
    }
}
