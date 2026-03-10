using Microsoft.AspNetCore.Mvc;
using Users.BLL;
using Users.DAL.Models;

namespace Users.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers([FromQuery] string? sortedBy, [FromQuery] string? filter)
        {
            // GET /users?sortedBy=...&filter=...
            var result = _service.GetUsers(sortedBy, filter);
            return Ok(result);
        }

        [HttpPost]
        public ActionResult<User> PostUser([FromBody] User user)
        {
            var created = _service.PostUser(user);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPatch("{id:guid}")]
        public ActionResult<User> PatchUser(Guid id, [FromBody] Dictionary<string, object> changes)
        {
            // PATCH /users/{id}
            var updated = _service.PatchUser(id, changes);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteUser(Guid id)
        {
            // DELETE /users/{id}
            // Verificar si existe para poder dar un error 404 si no
            var user = _service.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { error = true, message = "El usuario no existe." });
            }

            _service.DeleteUser(id);
            return Ok(new
            {
                success = true,
                message = $"Usuario con ID {id} eliminado correctamente."
            });
        }

        [HttpGet("{id:guid}")]
        public ActionResult<User> GetById(Guid id)
        {
            var user = _service.GetUserById(id);
            return user == null ? NotFound() : Ok(user);
        }

    }
}
