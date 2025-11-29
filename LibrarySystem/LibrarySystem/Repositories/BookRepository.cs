using LibrarySystem.API.DataContext;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibrarySystem.API.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;
        public BookRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Book> AddBookAsync(Book book)
        {
            var addedBook = await _context.Books.AddAsync(book);

            await _context.SaveChangesAsync();

            return addedBook.Entity;    
        }
        public async Task<BookAuthor> AddBookAuthorAsync(BookAuthor bookAuthor)
        {
            var added = await _context.BookAuthors.AddAsync(bookAuthor);

            await _context.SaveChangesAsync();
            return added.Entity;
        }
        public async Task<BookCopy> AddBookCopyAsync(BookCopy copy)
        {
            var addedBookCopy = await _context.BookCopies.AddAsync(copy);

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == copy.BookId);

            book.BookCopies.Add(addedBookCopy.Entity);

            await _context.SaveChangesAsync();

            return addedBookCopy.Entity;
        }
        public async Task<bool> DeleteBookAsync(int id)
        {
            var bookToDelete = await _context.Books.FindAsync(id);

            if (bookToDelete == null)
            {
                return false;
            }

            _context.Books.Remove(bookToDelete);
            int affectedRows = await _context.SaveChangesAsync();

            return affectedRows > 0;
        }
        public async Task<bool> DeleteBookCopyAsync(int id)
        {
            var copyToDelete = await _context.BookCopies.FindAsync(id);

            if (copyToDelete == null)
            {
                return false;
            }

            _context.BookCopies.Remove(copyToDelete);
            int affectedRows = await _context.SaveChangesAsync();

            return affectedRows > 0;
        } 
        public async Task<PaginatedResult<Book>> GetAllBooksAsync(BookFilterDto filterDto)
        {

            IQueryable<Book> query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookCopies)
                    .ThenInclude(bc => bc.Shelf)
                        .ThenInclude(s => s.Room);

            if (!string.IsNullOrWhiteSpace(filterDto.Title))
            {
                query = query.Where(b => b.Title != null && b.Title.ToLower().Contains(filterDto.Title.ToLower()));
            }

            if (filterDto.CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == filterDto.CategoryId.Value);
            }

            if (filterDto.PublicationYearFrom.HasValue)
            {
                query = query.Where(b => b.PublicationYear >= filterDto.PublicationYearFrom.Value);
            }

            if (filterDto.PublicationYearTo.HasValue)
            {
                query = query.Where(b => b.PublicationYear <= filterDto.PublicationYearTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterDto.Language))
            {
                query = query.Where(b => b.Language != null && b.Language.ToLower().Contains(filterDto.Language.ToLower()));
            }

            if (filterDto.PageCountMin.HasValue)
            {
                query = query.Where(b => b.PageCount >= filterDto.PageCountMin.Value);
            }

            if (filterDto.PageCountMax.HasValue)
            {
                query = query.Where(b => b.PageCount <= filterDto.PageCountMax.Value);
            }

            if (filterDto.HasAvailableCopy.GetValueOrDefault(false))
            {
                query = query.Where(b => b.BookCopies.Any(bc => bc.IsAvailable));
            }

            if (!string.IsNullOrWhiteSpace(filterDto.RoomCode))
            {
                string lowerRoomCode = filterDto.RoomCode.ToLower();
                query = query.Where(b => b.BookCopies.Any(bc =>
                    bc.Shelf != null &&
                    bc.Shelf.Room != null &&
                    bc.Shelf.Room.RoomCode != null && 
                    bc.Shelf.Room.RoomCode.ToLower() == lowerRoomCode));
            }

            var page = filterDto.Page.GetValueOrDefault(1);
            var size = filterDto.Size.GetValueOrDefault(12);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PaginatedResult<Book>(items, totalCount, page, size);
        }
        public async Task<Book?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);

            return book;
        }

        public async Task<Book?> GetBookByNameWithDetailsAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var query = _context.Books
                .FromSqlInterpolated($@"
                    SELECT * FROM Books 
                    WHERE 
                    DIFFERENCE(Title, {name}) >= 3 
                    OR 
                    SOUNDEX(Title) = SOUNDEX({name}) 
                    OR 
                    Title LIKE {'%' + name + '%'}
                ");

            var bookWithDetails = await query
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookCopies)
                    .ThenInclude(bc => bc.Shelf)
                        .ThenInclude(s => s.Room)
                .FirstOrDefaultAsync();

            return bookWithDetails;
        }

        public async Task<BookCopy?> GetBookCopyByBarcodeAsync(string barcode)
        {
            return await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BarcodeNumber == barcode);
        }
        public async Task<BookCopy?> GetBookCopyByIdAsync(int id)
        {
            return await _context.BookCopies.FindAsync(id);
        }

        public async Task<int> GetBookCountAsync()
        {
            return await _context.Books.CountAsync();
        }

        public async Task<Book?> GetBookWithDetailsAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookCopies)
                    .ThenInclude(bc => bc.Shelf)
                        .ThenInclude(s => s.Room)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Book>> GetOtherBooksByAuthorAsync(int authorId, int size, int? categoryId = null)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookCopies)
                    .ThenInclude(bc => bc.Shelf)
                        .ThenInclude(s => s.Room)
                .Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId));

            if (categoryId.HasValue)
            {
                query = query.OrderByDescending(b => b.CategoryId == categoryId.Value);
            }

            return await query
                .Take(size)
                .ToListAsync();
        }


        public async Task<bool> IsBookAuthorExistsAsync(int bookId, int authorId)
        {
            return await _context.BookAuthors
                .AnyAsync(ba => ba.BookId == bookId && ba.AuthorId == authorId);
        }
        public async Task<bool> SetBookCopyUnAvailableAsync(int bookCopyId)
        {
            return await _context.BookCopies
                .Where(bc => bc.Id == bookCopyId)
                .ExecuteUpdateAsync(bc => bc.SetProperty(b => b.IsAvailable, false)) > 0;
        }
        public async Task<Book> UpdateBookAsync(int id, Book book)
        {
            var existingBook = await _context.Books.FindAsync(id);

            if (existingBook == null)
            {
                throw new KeyNotFoundException($"Book with ID {id} not found.");
            }

            existingBook.Title = book.Title;
            existingBook.ISBN = book.ISBN;
            existingBook.PageCount = book.PageCount;
            existingBook.PublicationYear = book.PublicationYear;
            existingBook.Language = book.Language;
            existingBook.CategoryId = book.CategoryId;
            existingBook.PublisherId = book.PublisherId;

            await _context.SaveChangesAsync();

            return existingBook;
        }
        public async Task<BookCopy> UpdateBookCopyAsync(int id, BookCopy bookCopy)
        {
            var existingCopy = await _context.BookCopies.FindAsync(id);

            if (existingCopy == null)
            {
                throw new KeyNotFoundException($"Book Copy with ID {id} not found.");
            }

            existingCopy.BarcodeNumber = bookCopy.BarcodeNumber;
            existingCopy.IsAvailable = bookCopy.IsAvailable;
            existingCopy.BookId = bookCopy.BookId;
            existingCopy.ShelfId = bookCopy.ShelfId;

            await _context.SaveChangesAsync();

            return existingCopy;
        }
    }
}
