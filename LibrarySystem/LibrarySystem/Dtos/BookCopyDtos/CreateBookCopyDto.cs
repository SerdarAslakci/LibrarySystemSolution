using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.BookCopyDtos
{

    public class CreateBookCopyDto
    {
        [Required(ErrorMessage = "Kitap ID'si zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir Kitap ID'si giriniz.")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Barkod numarası zorunludur.")]
        [StringLength(50, ErrorMessage = "Barkod numarası 50 karakterden uzun olamaz.")]
        public string BarcodeNumber { get; set; }

        [Required(ErrorMessage = "Oda ID'si zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir Oda ID'si giriniz.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Raf kodu zorunludur.")]
        [StringLength(20, ErrorMessage = "Raf kodu 20 karakterden uzun olamaz.")]
        public string ShelfCode { get; set; }
    }
}
