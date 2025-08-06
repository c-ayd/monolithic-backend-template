using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Check()
        {
            return Ok();
        }
    }
}
