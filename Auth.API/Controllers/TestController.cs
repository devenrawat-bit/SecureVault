using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("Anyone with solana putta can access this");
        }

        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult Authenticated()
        {
            return Ok("You are authenticated.");
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet("super-admin")]
        public IActionResult SuperAdmin()
        {
            return Ok("You are a Super Admin.");
        }
    }
}
