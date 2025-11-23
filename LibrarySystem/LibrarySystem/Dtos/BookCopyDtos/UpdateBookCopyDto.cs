using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.BookCopyDtos
{
    public class UpdateBookCopyDto
    {

        [StringLength(50, ErrorMessage = "Barkod numarası 50 karakterden uzun olamaz.")]
        public string? BarcodeNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir Oda ID'si giriniz.")]
        public int? RoomId { get; set; }

        [StringLength(20, ErrorMessage = "Raf kodu 20 karakterden uzun olamaz.")]
        public string? ShelfCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir Kitap ID'si giriniz.")]
        public int? BookId { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
