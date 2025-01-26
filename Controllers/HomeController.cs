using FrizerskiSalon_VSITE.Models; 
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FrizerskiSalon_VSITE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET metoda za po?etnu stranicu
        public IActionResult Index()
        {
            _logger.LogInformation("Home page accessed.");
            return View();
        }

        // GET metoda za Privacy stranicu
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page accessed.");
            return View();
        }

        // Metoda za prikaz greške
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("An error occurred.");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
