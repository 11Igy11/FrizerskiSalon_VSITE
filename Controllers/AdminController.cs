using Microsoft.AspNetCore.Mvc;
using FrizerskiSalon_VSITE.Data;
using FrizerskiSalon_VSITE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace FrizerskiSalon_VSITE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // PRIKAZ ADMIN PANELA (USLUGE + OPCIJE)
        public IActionResult Index()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        // DODAVANJE USLUGE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // UREĐIVANJE USLUGE
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // BRISANJE USLUGE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            // Provjera postoji li rezervacija koja koristi ovu uslugu
            bool imaRezervacija = await _context.Reservations.AnyAsync(r => r.ServiceId == id);
            if (imaRezervacija)
            {
                TempData["ErrorMessage"] = "Usluga se koristi u rezervacijama i ne može se obrisati.";
                return RedirectToAction(nameof(Index));
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Usluga je uspješno obrisana.";
            return RedirectToAction(nameof(Index));
        }


        // PRIKAZ SVIH REZERVACIJA
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Service)
                .OrderBy(r => r.ReservationDate) // Prvo sortiramo po datumu
                .ThenBy(r => r.ReservationTime)  // Zatim po vremenu
                .ToListAsync();

            return View(reservations);
        }


        // BRISANJE REZERVACIJE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Reservations));
        }

        // PRIKAZ KORISNIKA (UPRAVLJANJE KORISNICIMA)
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // BRISANJE KORISNIKA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Brisanje korisnika nije uspjelo.");
                return RedirectToAction(nameof(Users));
            }

            return RedirectToAction(nameof(Users));
        }

        // UREĐIVANJE KORISNIKA
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(User user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null) return NotFound();

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;

            var result = await _userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }
    }
}
