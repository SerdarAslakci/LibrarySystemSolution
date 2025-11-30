using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.CategoryDtos
{
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
        public string? Name { get; set; }
    }
}
