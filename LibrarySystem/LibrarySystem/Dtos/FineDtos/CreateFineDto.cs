using System.ComponentModel.DataAnnotations;

public class CreateFineDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public string userId { get; set; }

    [Required(ErrorMessage = "fineTypeId zorunludur.")]
    [Range(1, int.MaxValue, ErrorMessage = "fineTypeId 1’den büyük olmalıdır.")]
    public int fineTypeId { get; set; }

    [Required(ErrorMessage = "Reason alanı boş olamaz.")]
    public string reason { get; set; }

    [Required(ErrorMessage = "Ceza miktarı zorunludur.")]
    [Range(0, 1_000_000, ErrorMessage = "Ceza miktarı 1 ile 1.000.000 arasında olmalıdır.")]
    public int amount { get; set; }
}
