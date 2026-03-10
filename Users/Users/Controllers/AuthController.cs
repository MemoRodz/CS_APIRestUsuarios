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
            //if (_service.Login(dto.TaxId, dto.Password))
            //    return Ok(new { success = true, message = "Login correcto" });
            //return Unauthorized(new { success = false, message = "Credenciales inválidas" });
            var ok = _service.Login(dto.TaxId, dto.Password);
            Console.WriteLine($"LoginAction:\n\tResultado Login: {ok}");
            if (!ok)
                return Unauthorized(new { success = false, message = "Credenciales inválidas" });

            return Ok(new { success = true, message = "Login correcto" });


        }

    }

}
