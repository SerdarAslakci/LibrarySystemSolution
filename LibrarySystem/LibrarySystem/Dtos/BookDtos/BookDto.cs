using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Dtos.BookDtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Isbn { get; set; }
        public int PageCount { get; set; }
        public int PublicationYear { get; set; }
        public string Language { get; set; }
        public string ImageUrl { get; set; }
        public string Summary { get; set; }

        public int CategoryId { get; set; }
        public int PublisherId { get; set; }

        public Category Category { get; set; }
        public Publisher Publisher { get; set; }

        public ICollection<BookAuthorDto> BookAuthors { get; set; }
        public ICollection<BookCopy> BookCopies { get; set; }
    }
}
