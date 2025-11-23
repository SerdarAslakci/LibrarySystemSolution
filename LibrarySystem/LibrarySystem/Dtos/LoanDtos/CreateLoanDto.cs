using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.LoanDtos
{
    public class CreateLoanDto
    {
            [Required]
            [StringLength(50, ErrorMessage = "Barkod maksimum 50 karakter olabilir.")]
            public string Barcode { get; set; }

            [Range(1, 365, ErrorMessage = "Ödünç süresi 1-365 gün arasında olmalıdır.")]
            public int LoanDays { get; set; } = 15;
    }
}
