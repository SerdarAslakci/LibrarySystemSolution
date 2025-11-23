using LibrarySystem.API.DataContext;
using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.ShelfDtos;
using LibrarySystem.API.Repositories;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorService _authorService;
        private readonly ICategoryService _categoryService;
        private readonly IPublisherService _publisherService;
        private readonly IShelfService _shelfService;
        private readonly IRoomService _roomService;

        public BookService(
            IBookRepository bookRepository,
            IAuthorService authorService,
            ICategoryService categoryService,
            IPublisherService publisherService,
            IShelfService shelfService,
            IRoomService roomService
            )
        {
            _bookRepository = bookRepository;
            _authorService = authorService;
            _categoryService = categoryService;
            _publisherService = publisherService;
            _shelfService = shelfService;
            _roomService = roomService;
        }

        public async Task<Book> AddBookAsync(CreateBookDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            Author author = await _authorService.GetOrCreateAsync(
                dto.AuthorId, dto.AuthorFirstName, dto.AuthorLastName);

            Category category = await _categoryService.GetOrCreateAsync(
                dto.CategoryId, dto.CategoryName);

            Publisher publisher = await _publisherService.GetOrCreateAsync(
                dto.PublisherId, dto.PublisherName);

            var book = new Book
            {
                Title = dto.Title ?? throw new ArgumentException("Kitap başlığı boş olamaz."),
                ISBN = dto.ISBN,
                PageCount = dto.PageCount,
                PublicationYear = dto.PublicationYear,
                Language = dto.Language,
                CategoryId = category.Id,
                PublisherId = publisher.Id,
                Category = category,
                Publisher = publisher
            };

            var addedBook = await _bookRepository.AddBookAsync(book);

            if (author != null)
            {
                var bookAuthor = new BookAuthor
                {
                    BookId = addedBook.Id,
                    AuthorId = author.Id
                };

                await this.AddBookAuthorAsync(bookAuthor);
            }

            return addedBook;
        }
        public async Task<BookAuthor> AddBookAuthorAsync(BookAuthor bookAuthor)
        {
            if (bookAuthor == null)
                throw new ArgumentNullException(nameof(bookAuthor));

            if (bookAuthor.BookId <= 0 || bookAuthor.AuthorId <= 0)
                throw new ArgumentException("Geçerli Kitap veya Yazar ID'si belirtilmelidir.");

            bool exists = await _bookRepository.IsBookAuthorExistsAsync(
                bookAuthor.BookId,
                bookAuthor.AuthorId
            );

            if (exists)
            {
                throw new InvalidOperationException(
                    $"Kitap ID'si {bookAuthor.BookId} ve Yazar ID'si {bookAuthor.AuthorId} olan ilişki zaten mevcut."
                );
            }


            var added = await _bookRepository.AddBookAuthorAsync(bookAuthor);

            return added;
        }
        public async Task<BookCopy> AddBookCopyAsync(CreateBookCopyDto createBookCopyDto)
        {

            var book = await _bookRepository.GetBookByIdAsync(createBookCopyDto.BookId);
            if (book == null)
                throw new KeyNotFoundException($"ID {createBookCopyDto.BookId} ile kayıtlı bir kitap bulunamadı.");

            var room = await _roomService.GetRoomByIdAsync(createBookCopyDto.RoomId);
            if (room == null)
                throw new KeyNotFoundException($"ID {createBookCopyDto.RoomId} ile kayıtlı bir oda bulunamadı.");

            var shelf = await _shelfService.GetShelfByCodeAndRoomIdAsync(
                createBookCopyDto.ShelfCode,
                createBookCopyDto.RoomId);

            if (shelf == null)
            {
                var shelfDto = new CreateShelfDto
                {
                    ShelfCode = createBookCopyDto.ShelfCode,
                    RoomId = createBookCopyDto.RoomId,
                };

                shelf = await _shelfService.AddShelfAsync(shelfDto);
            }

            var bookCopy = new BookCopy
            {
                BookId = createBookCopyDto.BookId,
                BarcodeNumber = createBookCopyDto.BarcodeNumber,
                ShelfId = shelf.Id,
                IsAvailable = true
            };

            return await _bookRepository.AddBookCopyAsync(bookCopy);
        }



        public async Task<Book> UpdateBookAsync(int id, CreateBookDto updateBookDto)
        {
            if (updateBookDto == null)
                throw new ArgumentNullException(nameof(updateBookDto));

            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap bulunamadı.");

            Author author = await _authorService.GetOrCreateAsync(
                updateBookDto.AuthorId, updateBookDto.AuthorFirstName, updateBookDto.AuthorLastName);

            Category category = await _categoryService.GetOrCreateAsync(
                updateBookDto.CategoryId, updateBookDto.CategoryName);

            Publisher publisher = await _publisherService.GetOrCreateAsync(
                updateBookDto.PublisherId, updateBookDto.PublisherName);

            var bookToUpdate = new Book
            {
                Title = updateBookDto.Title ?? throw new ArgumentException("Kitap başlığı boş olamaz."),
                ISBN = updateBookDto.ISBN,
                PageCount = updateBookDto.PageCount,
                PublicationYear = updateBookDto.PublicationYear,
                Language = updateBookDto.Language,
                CategoryId = category.Id,
                PublisherId = publisher.Id
            };

            var updatedBook = await _bookRepository.UpdateBookAsync(id, bookToUpdate);

            if (author != null)
            {
                bool authorExists = await _bookRepository.IsBookAuthorExistsAsync(id, author.Id);
                if (!authorExists)
                {
                    var bookAuthor = new BookAuthor
                    {
                        BookId = id,
                        AuthorId = author.Id
                    };
                    await this.AddBookAuthorAsync(bookAuthor);
                }
            }

            return updatedBook;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap bulunamadı.");

            return await _bookRepository.DeleteBookAsync(id);
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçerli bir kitap ID'si belirtilmelidir.", nameof(id));

            return await _bookRepository.GetBookByIdAsync(id);
        }

        public async Task<Book?> GetBookWithDetailsAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçerli bir kitap ID'si belirtilmelidir.", nameof(id));

            return await _bookRepository.GetBookWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync(BookFilterDto filterDto)
        {
            return await _bookRepository.GetAllBooksAsync(filterDto);
        }

        public async Task<bool> IsBookAuthorExistsAsync(int bookId, int authorId)
        {
            if (bookId <= 0 || authorId <= 0)
                throw new ArgumentException("Geçerli Kitap ve Yazar ID'leri belirtilmelidir.");

            return await _bookRepository.IsBookAuthorExistsAsync(bookId, authorId);
        }

        public async Task<BookCopy> UpdateBookCopyAsync(int id, UpdateBookCopyDto updateBookCopyDto)
        {
            if (updateBookCopyDto == null)
                throw new ArgumentNullException(nameof(updateBookCopyDto));

            var existingCopy = await _bookRepository.GetBookCopyByIdAsync(id);
            if (existingCopy == null)
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap kopyası bulunamadı.");

            var bookCopyToUpdate = new BookCopy
            {
                BookId = updateBookCopyDto.BookId ?? existingCopy.BookId,
                BarcodeNumber = updateBookCopyDto.BarcodeNumber ?? existingCopy.BarcodeNumber,
                IsAvailable = updateBookCopyDto.IsAvailable ?? existingCopy.IsAvailable
            };

            if (updateBookCopyDto.ShelfCode != null && updateBookCopyDto.RoomId.HasValue)
            {
                var shelf = await _shelfService.GetShelfByCodeAndRoomIdAsync(
                    updateBookCopyDto.ShelfCode,
                    updateBookCopyDto.RoomId.Value);

                bookCopyToUpdate.ShelfId = shelf.Id;
            }
            else
            {
                bookCopyToUpdate.ShelfId = existingCopy.ShelfId;
            }

            if (updateBookCopyDto.BookId.HasValue)
            {
                var bookExists = await _bookRepository.GetBookByIdAsync(updateBookCopyDto.BookId.Value);
                if (bookExists == null)
                    throw new KeyNotFoundException($"ID {updateBookCopyDto.BookId.Value} ile kayıtlı ana kitap bulunamadı.");
            }

            return await _bookRepository.UpdateBookCopyAsync(id, bookCopyToUpdate);
        }

        public async Task<bool> DeleteBookCopyAsync(int id)
        {
            var existingCopy = await _bookRepository.GetBookCopyByIdAsync(id);
            if (existingCopy == null)
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap kopyası bulunamadı.");

            return await _bookRepository.DeleteBookCopyAsync(id);
        }

        public async Task<BookCopy?> GetBookCopyByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("Barkod boş olamaz.", nameof(barcode));

            var bookCopy = await _bookRepository.GetBookCopyByBarcodeAsync(barcode);

            if (bookCopy == null)
                throw new KeyNotFoundException($"Barkod '{barcode}' ile eşleşen kitap bulunamadı.");

            return bookCopy;
        }

        public async Task<bool> SetBookCopyUnAvailableAsync(int bookCopyId)
        {
            return await _bookRepository.SetBookCopyUnAvailableAsync(bookCopyId);
        }
    }

}
