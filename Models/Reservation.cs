using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrizerskiSalon_VSITE.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime korisnika je obavezno.")]
        public string? CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum je obavezan.")]
        [DataType(DataType.Date, ErrorMessage = "Neispravan format datuma.")]
        public DateTime ReservationDate { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        [Required(ErrorMessage = "Usluga je obavezna.")]
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; } = null!;
    }
}
