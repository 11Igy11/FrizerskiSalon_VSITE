using System.ComponentModel.DataAnnotations;

namespace FrizerskiSalon_VSITE.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv usluge je obavezan.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cijena usluge je obavezna.")]
        [Range(0, double.MaxValue, ErrorMessage = "Cijena mora biti veća od 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Opis usluge je obavezan.")]
        public string Description { get; set; }  // Dodano polje

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
