using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrizerskiSalon_VSITE.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime korisnika je obavezno.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum je obavezan.")]
        [DataType(DataType.Date, ErrorMessage = "Neispravan format datuma.")]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Vrijeme termina je obavezno.")]
        public string TimeSlot { get; set; } = string.Empty;

        // Strani ključ za korisnika – sada više NIJE obavezan
        public string? UserId { get; set; }

        public User? User { get; set; } // Navigacijsko svojstvo za korisnika

        // Strani ključ za uslugu
        [Required]
        public int ServiceId { get; set; }

        public Service? Service { get; set; } // Navigacijsko svojstvo za uslugu

        public string? Notes { get; set; }
    }
}
