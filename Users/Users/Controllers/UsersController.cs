using Microsoft.AspNetCore.Mvc;

namespace Users.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
