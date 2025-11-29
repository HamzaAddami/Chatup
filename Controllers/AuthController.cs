using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Chatup.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Guid Id = Guid.NewGuid();

        [HttpGet]
        public IActionResult GetId()
        {
            string response = this.Id.ToString();
            return Ok(response);
        }

    }
}
