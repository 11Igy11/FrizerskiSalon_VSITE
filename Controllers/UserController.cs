using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FrizerskiSalon_VSITE.Models;
using System.Threading.Tasks;

namespace FrizerskiSalon_VSITE.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);
                if (result.Succeeded)
                {
                    // Preusmjeravanje po ulogama
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    return RedirectToAction("Index", "User");
                }
                ModelState.AddModelError("", "Pogrešna lozinka.");
            }
            else
            {
                ModelState.AddModelError("", "Korisnik ne postoji.");
            }
            return View();
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    Name = model.Name
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Dodavanje uloga
                    if (model.Email.ToLower() == "admin@frizerski-salon.hr")
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "User");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        // GET: User Dashboard
        public IActionResult Index()
        {
            return View();
        }
    }
}
