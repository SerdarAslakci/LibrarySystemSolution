using System.ComponentModel.DataAnnotations;

public class AuthorBooksRequestDto
{
    [Required(ErrorMessage = "AuthorId boş olamaz.")]
    public int AuthorId { get; set; }
    [Range(1, 200, ErrorMessage = "Size 1 ile 200 arasında olmalıdır.")]
    public int? Size { get; set; } = 5;
    public int? CategoryId { get; set; }
}
