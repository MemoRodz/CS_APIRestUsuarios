using Microsoft.AspNetCore.Mvc;

namespace Users.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
