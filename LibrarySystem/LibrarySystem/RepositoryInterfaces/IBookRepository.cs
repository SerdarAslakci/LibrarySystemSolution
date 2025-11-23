using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.Models.Models;
using System.Diagnostics.Metrics;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksAsync(BookFilterDto filterDto);
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book?> GetBookWithDetailsAsync(int id);
        Task<BookCopy?> GetBookCopyByIdAsync(int id);
        Task<BookCopy?> GetBookCopyByBarcodeAsync(string barcode);

        Task<Book> AddBookAsync(Book book);
        Task<BookCopy> AddBookCopyAsync(BookCopy copy);
        Task<BookAuthor> AddBookAuthorAsync(BookAuthor bookAuthor);
        Task<bool> IsBookAuthorExistsAsync(int bookId, int authorId);
        Task<Book> UpdateBookAsync(int id, Book book);
        Task<BookCopy> UpdateBookCopyAsync(int id, BookCopy bookCopy);
        Task<bool> SetBookCopyUnAvailableAsync(int bookCopyId);

        Task<bool> DeleteBookAsync(int id);
        Task<bool> DeleteBookCopyAsync(int id);


    }
}
