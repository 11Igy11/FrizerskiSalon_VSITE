using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrizerskiSalon_VSITE.Controllers
{
    [Authorize(Roles = "User")]
    public class UserPanelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
