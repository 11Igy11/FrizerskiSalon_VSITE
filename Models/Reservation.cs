using System.ComponentModel.DataAnnotations;

namespace FrizerskiSalon_VSITE.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        //  [Required] za obavezna polja
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required]
        public string TimeSlot { get; set; } = string.Empty;

        // Strani ključ za korisnika (treba biti string ako koristite IdentityUser)
        public string UserId { get; set; }  // Treba biti string, ne int, ako koristite IdentityUser
        public User User { get; set; } = null!;
    }
}
