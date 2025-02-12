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
    .OrderBy(r => r.ReservationDate)
    .ThenBy(r => r.ReservationTime)
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
                ModelState.AddModelError("ReservationDate", "Morate odabrati datum i vrijeme.");
            }
            // Provjera da li je datum i vrijeme u prošlosti
            DateTime now = DateTime.Now;
            DateTime selectedDateTime = reservation.ReservationDate.Date + reservation.ReservationTime;

            if (selectedDateTime < now)
            {
                ModelState.AddModelError("ReservationDate", "Ne možete rezervirati termin u prošlosti.");
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/CreateReservation.cshtml", reservation);
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

                // Parsiranje trajanja usluge iz `Description`
                int duration = ExtractDurationFromDescription(service.Description);
                if (duration == 0)
                {
                    ModelState.AddModelError("ServiceId", "Neispravan format trajanja usluge.");
                }

                // Postavljanje vremena završetka rezervacije
                DateTime endTime = reservation.ReservationDate.AddMinutes(duration);

                // Provjera preklapanja termina
                var sveRezervacije = await _context.Reservations.Include(r => r.Service).ToListAsync();

                bool terminZauzet = sveRezervacije.Any(r =>
    r.ReservationDate.Date == reservation.ReservationDate.Date &&  // Mora biti isti datum
    (r.ReservationTime < reservation.ReservationTime + TimeSpan.FromMinutes(duration) &&
     reservation.ReservationTime < r.ReservationTime + TimeSpan.FromMinutes(ExtractDurationFromDescription(r.Service.Description)))
);




                if (terminZauzet)
                {
                    ModelState.AddModelError("ReservationDate", "Odabrani termin nije dostupan.");
                }
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
        private int ExtractDurationFromDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return 0;

            var words = description.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int duration))
                {
                    return duration; // Prvi broj u stringu se uzima kao trajanje u minutama
                }
            }
            return 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(DateTime date, int serviceId)
        {
            // Dohvati sve rezervacije za odabrani datum
            var rezervacijeZaDan = await _context.Reservations
                .Where(r => r.ReservationDate.Date == date.Date)
                .Include(r => r.Service)
                .ToListAsync();

            // Dohvati trajanje odabrane usluge
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return Json(new { error = "Odabrana usluga ne postoji." });
            }

            int duration = ExtractDurationFromDescription(service.Description);
            if (duration == 0)
            {
                return Json(new { error = "Neispravan format trajanja usluge." });
            }

            // Generiraj sve moguće termine od 08:00 do 20:00 u koracima od 30 minuta
            List<string> sviTermini = GenerateTimeSlots(8, 20, 30);

            // Izračunaj zauzete termine
            List<string> zauzetiTermini = rezervacijeZaDan
    .Select(r => r.ReservationTime.ToString(@"hh\:mm"))
    .ToList();


            // Filtriraj samo slobodne termine
            var slobodniTermini = sviTermini.Except(zauzetiTermini).ToList();

            return Json(slobodniTermini);
        }

        // Funkcija za generiranje termina svakih X minuta između početnog i krajnjeg vremena
        private List<string> GenerateTimeSlots(int startHour, int endHour, int interval)
        {
            List<string> timeSlots = new List<string>();
            DateTime startTime = DateTime.Today.AddHours(startHour);

            while (startTime.Hour < endHour)
            {
                timeSlots.Add(startTime.ToString("HH:mm"));
                startTime = startTime.AddMinutes(interval);
            }

            return timeSlots;
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
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            var existingReservation = await _context.Reservations.FindAsync(id);
            if (existingReservation == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(reservation.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Odabrana usluga ne postoji.");
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            // ✅ Spriječiti rezervaciju u prošlosti
            DateTime now = DateTime.Now;
            DateTime selectedDateTime = reservation.ReservationDate.Date + reservation.ReservationTime;

            if (selectedDateTime < now)
            {
                ModelState.AddModelError("ReservationDate", "Ne možete rezervirati termin u prošlosti.");
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            // ✅ Spriječiti preklapanje termina
            int duration = ExtractDurationFromDescription(service.Description);
            TimeSpan newEndTime = reservation.ReservationTime + TimeSpan.FromMinutes(duration);

            var conflictingReservations = await _context.Reservations
                .Where(r => r.ReservationDate.Date == reservation.ReservationDate.Date && r.Id != reservation.Id)
                .Include(r => r.Service)
                .ToListAsync();

            bool terminZauzet = conflictingReservations.Any(r =>
                r.ReservationTime < newEndTime &&
                reservation.ReservationTime < r.ReservationTime + TimeSpan.FromMinutes(ExtractDurationFromDescription(r.Service.Description))
            );

            if (terminZauzet)
            {
                ModelState.AddModelError("ReservationDate", "Odabrani termin nije dostupan.");
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            // ✅ Ažuriranje rezervacije
            existingReservation.CustomerName = reservation.CustomerName;
            existingReservation.ReservationDate = reservation.ReservationDate;
            existingReservation.ReservationTime = reservation.ReservationTime;
            existingReservation.ServiceId = reservation.ServiceId;
            existingReservation.UserId = userId;
            existingReservation.Service = service;
            ModelState.Remove("Service");

            if (!ModelState.IsValid)
            {
                ViewBag.Services = new SelectList(_context.Services, "Id", "Name", reservation.ServiceId);
                return View("~/Views/User/EditReservation.cshtml", reservation);
            }

            _context.Update(existingReservation);
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
