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
            return CreatedAtAction(nameof(GetUsers), new { }, created);
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
            _service.DeleteUser(id);
            return NoContent();
        }

    }
}
