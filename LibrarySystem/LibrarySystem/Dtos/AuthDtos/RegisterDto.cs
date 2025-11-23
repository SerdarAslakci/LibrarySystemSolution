using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.AuthDtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Telefon numarası alanı zorunludur.")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
            ErrorMessage = "Geçerli bir telefon numarası giriniz (Örn: 5xx xxx xx xx).")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola alanı zorunludur.")]
        [MinLength(8, ErrorMessage = "Parola en az 8 karakter uzunluğunda olmalıdır.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Doğum Tarihi alanı zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
    }
}
