using Microsoft.AspNetCore.Mvc;
using FrizerskiSalon_VSITE.Data;
using FrizerskiSalon_VSITE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrizerskiSalon_VSITE.Controllers
{
    [Authorize(Roles = "Admin")] // Ovaj atribut osigurava da samo administrator ima pristup
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin panel početna stranica (prikaz svih usluga)
        public IActionResult Index()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        // GET: Dodavanje nove usluge
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dodavanje nove usluge
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

        // GET: Uređivanje usluge
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound(); // Vraća status 404 ako usluga nije pronađena
            }
            return View(service);
        }

        // POST: Uređivanje usluge
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service service)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Services.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Services.Any(s => s.Id == service.Id))
                    {
                        return NotFound(); // Vraća status 404 ako usluga više ne postoji
                    }
                    else
                    {
                        throw; // Ako je greška drugačija, baci iznimku
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Potvrda brisanja usluge
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Brisanje usluge
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Dodavanje rezervacije
        public IActionResult CreateReservation()
        {
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name"); // Popunjavamo izbornik s uslugama
            return View();
        }

        // POST: Dodavanje rezervacije
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Reservations)); // Vraća se na pregled rezervacija
            }

            ViewBag.Services = new SelectList(_context.Services, "Id", "Name"); // Ponovno punimo izbornik ako validacija nije uspjela
            return View(reservation);
        }


        // GET: Prikaz svih rezervacija
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)    // Učitava povezane korisnike
                .Include(r => r.Service) // Učitava povezane usluge
                .ToListAsync();
            return View(reservations);
        }

        // GET: Uređivanje rezervacije
        public async Task<IActionResult> EditReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User) // Uključuje korisnika
                .Include(r => r.Service) // Uključuje uslugu
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }
            return View(reservation);
        }

        // POST: Uređivanje rezervacije
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReservation(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Reservations.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reservations.Any(r => r.Id == reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Reservations));
            }
            return View(reservation);
        }

        // GET: Potvrda brisanja rezervacije
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }
            return View(reservation);
        }

        // POST: Brisanje rezervacije
        [HttpPost, ActionName("DeleteReservation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReservationConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reservations));
        }
    }
}
