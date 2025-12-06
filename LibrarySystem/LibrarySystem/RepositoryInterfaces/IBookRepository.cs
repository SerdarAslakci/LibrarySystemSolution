using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.Models.Models;
using System.Diagnostics.Metrics;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IBookRepository
    {
        Task<PaginatedResult<BookDto>> GetAllBooksAsync(BookFilterDto filterDto);
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book?> GetBookWithDetailsAsync(int id);
        Task<BookCopy?> GetBookCopyByIdAsync(int id);
        Task<IEnumerable<BookCopy>> GetAllBookCopiesAsync(int bookId, int page, int pageSize);
        Task<BookCopy?> GetBookCopyByBarcodeAsync(string barcode);
        Task<IEnumerable<Book>?> GetBooksByNameWithDetailsAsync(string name);
        Task<IEnumerable<Book>> GetOtherBooksByAuthorAsync(int authorId, int size, int? categoryId = null);
        Task<BookAuthor?> GetBookAuthorByBookIdAsync(int bookId);

        Task<int> GetBookCountAsync();


        Task<Book> AddBookAsync(Book book);
        Task<BookCopy> AddBookCopyAsync(BookCopy copy);
        Task<BookAuthor> AddBookAuthorAsync(BookAuthor bookAuthor);
        Task<bool> IsBookAuthorExistsAsync(int bookId, int authorId);
        Task<Book> UpdateBookAsync(int id, Book book);
        Task<BookCopy> UpdateBookCopyAsync(int id, BookCopy bookCopy);
        Task DeleteBookAuthorRelationAsync(BookAuthor bookAuthor);
        Task<bool> SetBookCopyUnAvailableAsync(int bookCopyId);

        Task<bool> DeleteBookAsync(int id);
        Task<bool> DeleteBookCopyAsync(int id);


    }
}
