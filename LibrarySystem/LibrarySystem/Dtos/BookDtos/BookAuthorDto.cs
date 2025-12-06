using LibrarySystem.API.Dtos.AuthorDtos;

namespace LibrarySystem.API.Dtos.BookDtos
{
    public class BookAuthorDto
    {
        public int BookId { get; set; }
        public int AuthorId { get; set; }
        public AuthorDto Author { get; set; }
    }
}
