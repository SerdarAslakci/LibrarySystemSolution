using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.FineTypeDtos
{
    public class CreateFineTypeDto
    {
        [Required(ErrorMessage = "Ceza türü adı boş bırakılamaz.")]
        [MaxLength(50, ErrorMessage = "Ceza türü adı 50 karakterden uzun olamaz.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Günlük ücret alanı zorunludur.")]
        [Range(0, 10000, ErrorMessage = "Günlük ücret 0 ile 10000 arasında olmalıdır.")]
        public decimal DailyRate { get; set; }
    }
}
