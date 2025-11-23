using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.AuthDtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi formatı giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola alanı zorunludur.")]
        public string Password { get; set; }
    }
}
