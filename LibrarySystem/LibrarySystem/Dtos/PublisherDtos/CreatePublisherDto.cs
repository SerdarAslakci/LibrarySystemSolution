using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.Dtos.PublisherDtos
{
    public class CreatePublisherDto
    {
        [Required(ErrorMessage = "İsim alanı zorunludur.")]
        public string Name { get; set; }
    }
}
