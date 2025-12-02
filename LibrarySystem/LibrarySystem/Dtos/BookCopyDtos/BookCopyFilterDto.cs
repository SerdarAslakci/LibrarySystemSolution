using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.BookCopyDtos
{
    public class BookCopyFilterDto
    {
        [Required(ErrorMessage = "Kitap ID'si (BookId) gereklidir.")]
        public int BookId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası (page) 1 veya daha büyük olmalıdır.")]
        public int page { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Sayfa boyutu (pageSize) 1 veya daha büyük olmalıdır.")]
        public int pageSize { get; set; } = 10;
    }
}
