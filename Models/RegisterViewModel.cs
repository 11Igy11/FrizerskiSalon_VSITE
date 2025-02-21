using System.ComponentModel.DataAnnotations;

namespace FrizerskiSalon_VSITE.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Neispravan format email adrese.")]
        [Display(Name = "Email adresa")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Lozinke se ne podudaraju.")]
        [Display(Name = "Potvrdi lozinku")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Ime i prezime su obavezni.")]
        [Display(Name = "Ime i Prezime")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Telefon je obavezan.")]
        [Phone(ErrorMessage = "Neispravan format telefonskog broja.")]
        [Display(Name = "Telefonski broj")]
        public string Phone { get; set; }
    }
}
