namespace LibrarySystem.API.Dtos.BookDtos
{
    public class CreateBookDto
    {
        public string? Title { get; set; }
        public string? ISBN { get; set; }
        public int PageCount { get; set; }
        public int PublicationYear { get; set; }
        public string? Language { get; set; }

        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }

        public int? AuthorId { get; set; }
        public string? AuthorFirstName { get; set; }
        public string? AuthorLastName { get; set; }

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int? PublisherId { get; set; }
        public string? PublisherName { get; set; }

    }

}
