using Microsoft.AspNetCore.Mvc;
using Users.BLL;

namespace Users.Controllers
{
    public record LoginRequest(string TaxId, string Password);

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _service;

        public AuthController(UserService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest dto)
        {
            // POST /login
            var ok = _service.Login(dto.TaxId, dto.Password);
            if (!ok)
                return Unauthorized(new { success = false, message = "Credenciales inválidas" });

            return Ok(new { success = true, message = "Login correcto" });


        }

    }

}
