using Microsoft.AspNetCore.Mvc;
using FrizerskiSalon_VSITE.Data;
using FrizerskiSalon_VSITE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FrizerskiSalon_VSITE.Controllers
{
    [Authorize(Roles = "User")]
    public class UserPanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserPanelController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Početna stranica korisnika
        public IActionResult Index()
        {
            return View();
        }

        // GET: Prikaz korisničkih rezervacija
        public async Task<IActionResult> Reservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Service)
                .ToListAsync();

            return View("~/Views/User/Reservations.cshtml", reservations);
        }

        // GET: Kreiranje nove rezervacije
        public IActionResult CreateReservation()
        {
            var services = _context.Services.ToList();

            if (!services.Any())
            {
                ModelState.AddModelError("", "Nema dostupnih usluga. Kontaktirajte administratora.");
                ViewBag.Services = new SelectList(new List<Service>());
            }
            else
            {
                ViewBag.Services = new SelectList(services, "Id", "Name");
            }

            return View("~/Views/User/CreateReservation.cshtml");
        }

        // POST: Kreiranje rezervacije
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation([Bind("CustomerName, ReservationDate, TimeSlot, ServiceId")] Reservation reservation)
        {
            Console.WriteLine("START - Kreiranje rezervacije");

            reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"UserID postavljen: {reservation.UserId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR - ModelState nije validan!");
                foreach (var modelError in ModelState)
                {
                    Console.WriteLine($"Ključ: {modelError.Key}, Greška: {string.Join(", ", modelError.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
                return View("~/Views/User/CreateReservation.cshtml", reservation);
            }

            var service = await _context.Services.FindAsync(reservation.ServiceId);
            if (service == null)
            {
                Console.WriteLine("ERROR - Usluga nije pronađena!");
                ModelState.AddModelError("", "Odabrana usluga ne postoji.");
                return View("~/Views/User/CreateReservation.cshtml", reservation);
            }

            reservation.ReservationDate = reservation.ReservationDate.Date;

            Console.WriteLine("Dodavanje rezervacije u bazu...");
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            Console.WriteLine("USPJEH - Rezervacija spremljena!");

            return RedirectToAction(nameof(Reservations));
        }

        // GET: Uređivanje rezervacije
        public async Task<IActionResult> EditReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
            return View("~/Views/User/EditReservation.cshtml", reservation);
        }

        // POST: Uređivanje rezervacije
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReservation(int id, Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            // Ručno postavi UserId kako ne bi bio NULL
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Došlo je do greške pri identifikaciji korisnika.");
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            reservation.UserId = userId; // Sada UserId neće biti NULL

            if (!ModelState.IsValid)
            {
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            try
            {
                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
                Console.WriteLine("USPJEH - Rezervacija ažurirana!");
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


        // GET: Potvrda brisanja rezervacije
        // GET: Potvrda brisanja rezervacije
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Service) // Dodano kako bi učitali Service
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View("~/Views/User/DeleteReservation.cshtml", reservation);
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
