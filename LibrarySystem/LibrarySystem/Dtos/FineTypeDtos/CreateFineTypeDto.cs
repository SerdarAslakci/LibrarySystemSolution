using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.FineTypeDtos
{
    public class CreateFineTypeDto
    {
        [Required(ErrorMessage = "Ceza türü adı boş bırakılamaz.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Günlük ücret alanı zorunludur.")]
        public decimal DailyRate { get; set; }
    }
}
