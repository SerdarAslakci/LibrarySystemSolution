using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IBookService
    {
        // Book Operations
        Task<Book> AddBookAsync(CreateBookDto createBookDto);
        Task<Book> UpdateBookAsync(int id, CreateBookDto updateBookDto);
        Task<bool> DeleteBookAsync(int id);
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book?> GetBookWithDetailsAsync(int id);
        Task<Book?> GetBookByNameWithDetailsAsync(string name);
        Task<PaginatedResult<Book>> GetAllBooksAsync(BookFilterDto filterDto);
        Task<IEnumerable<Book>> GetOtherBooksByAuthorAsync(int authorId, int? size, int? categoryId = null);

        // BookAuthor Operations
        Task<BookAuthor> AddBookAuthorAsync(BookAuthor bookAuthor);
        Task<bool> IsBookAuthorExistsAsync(int bookId, int authorId);

        // BookCopy Operations
        Task<BookCopy?> GetBookCopyByBarcodeAsync(string barcode);

       
        Task<BookCopy> AddBookCopyAsync(CreateBookCopyDto createBookCopyDto);
        Task<BookCopy> UpdateBookCopyAsync(int id, UpdateBookCopyDto updateBookCopyDto);
        Task<bool> SetBookCopyUnAvailableAsync(int bookCopyId);
        Task<bool> DeleteBookCopyAsync(int id);
    }
}
