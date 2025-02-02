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

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Reservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Service)
                .ToListAsync();

            return View("~/Views/User/Reservations.cshtml", reservations ?? new List<Reservation>());
        }

        public IActionResult CreateReservation()
        {
            var services = _context.Services.ToList();

            if (!services.Any())
            {
                TempData["ErrorMessage"] = "Nema dostupnih usluga!";
                return RedirectToAction(nameof(Reservations));
            }

            ViewBag.Services = new SelectList(services, "Id", "Name");
            return View("~/Views/User/CreateReservation.cshtml", new Reservation());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReservation(Reservation reservation)
        {
            reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(reservation.UserId))
            {
                ModelState.AddModelError("UserId", "Došlo je do greške pri identifikaciji korisnika.");
            }

            if (string.IsNullOrEmpty(reservation.CustomerName))
            {
                var user = await _context.Users.FindAsync(reservation.UserId);
                if (user != null)
                {
                    reservation.CustomerName = user.Name;
                }
                else
                {
                    ModelState.AddModelError("CustomerName", "Ime korisnika je obavezno.");
                }
            }

            if (reservation.ServiceId <= 0)
            {
                ModelState.AddModelError("ServiceId", "Morate odabrati uslugu.");
            }

            if (reservation.ReservationDate == DateTime.MinValue)
            {
                ModelState.AddModelError("ReservationDate", "Morate odabrati datum.");
            }

            var service = await _context.Services.FindAsync(reservation.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Odabrana usluga ne postoji.");
            }
            else
            {
                reservation.Service = service;
                ModelState.Remove("Service");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
                return View("~/Views/User/CreateReservation.cshtml", reservation);
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rezervacija uspješno kreirana!";
            return RedirectToAction(nameof(Reservations));
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReservation(int id, Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Došlo je do greške pri identifikaciji korisnika.");
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View(reservation);
            }

            reservation.UserId = userId;

            if (!ModelState.IsValid)
            {
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Reservations));
        }

        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.Service).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View("~/Views/User/DeleteReservation.cshtml", reservation);
        }

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
