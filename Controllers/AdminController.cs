using Microsoft.AspNetCore.Mvc;
using FrizerskiSalon_VSITE.Data;
using FrizerskiSalon_VSITE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FrizerskiSalon_VSITE.Controllers
{
    [Authorize(Roles = "Admin")]  // Ovaj atribut osigurava da samo administrator ima pristup
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET metoda za prikaz početne stranice admin panela
        public IActionResult Index()
        {
            var services = _context.Services.ToList();
            return View(services);  // Prikaz svih usluga na početnoj stranici admin panela
        }

        // GET metoda za dodavanje nove usluge
        public IActionResult Create()
        {
            return View();
        }

        // POST metoda za dodavanje nove usluge
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));  // Povratak na Index stranicu admin panela
            }
            return View(service);
        }
    }
}
