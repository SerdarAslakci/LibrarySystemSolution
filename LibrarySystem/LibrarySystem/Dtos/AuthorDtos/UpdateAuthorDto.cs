using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.AuthorDtos
{
    public class UpdateAuthorDto
    {
        [Required(ErrorMessage = "Yazarın adı boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "Yazarın adı en fazla 100 karakter olabilir.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Yazarın soyadı boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "Yazarın soyadı en fazla 100 karakter olabilir.")]
        public string? LastName { get; set; }
    }
}
