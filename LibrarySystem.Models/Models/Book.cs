using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem.Models.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ISBN { get; set; }
        public int PageCount { get; set; }
        public int PublicationYear { get; set; } 
        public string? Language { get; set; }
        public string? ImageUrl { get; set; }
        public string? Summary { get; set; }
        public int CategoryId { get; set; }
        public int PublisherId { get; set; }
        public Category? Category { get; set; }
        public Publisher? Publisher { get; set; }
        public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }
}
