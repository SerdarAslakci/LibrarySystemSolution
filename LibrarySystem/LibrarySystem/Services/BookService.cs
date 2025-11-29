using Azure.Core;
using LibrarySystem.API.DataContext;
using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.ShelfDtos;
using LibrarySystem.API.Repositories;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<BookService> _logger;

        public BookService(
            IBookRepository bookRepository,
            IAuthorService authorService,
            ICategoryService categoryService,
            IPublisherService publisherService,
            IShelfService shelfService,
            IRoomService roomService,
            ILogger<BookService> logger
            )
        {
            _bookRepository = bookRepository;
            _authorService = authorService;
            _categoryService = categoryService;
            _publisherService = publisherService;
            _shelfService = shelfService;
            _roomService = roomService;
            _logger = logger;
        }

        public async Task<Book> AddBookAsync(CreateBookDto dto)
        {
            _logger.LogInformation("Yeni kitap ekleme işlemi başlatıldı. Başlık: {Title}, ISBN: {ISBN}", dto?.Title, dto?.ISBN);

            if (dto == null)
            {
                _logger.LogWarning("Kitap ekleme başarısız: DTO boş.");
                throw new ArgumentNullException(nameof(dto));
            }

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

            _logger.LogInformation("Kitap başarıyla eklendi. ID: {BookId}, Başlık: {Title}", addedBook.Id, addedBook.Title);

            return addedBook;
        }
        public async Task<BookAuthor> AddBookAuthorAsync(BookAuthor bookAuthor)
        {
            if (bookAuthor == null)
                throw new ArgumentNullException(nameof(bookAuthor));

            if (bookAuthor.BookId <= 0 || bookAuthor.AuthorId <= 0)
            {
                _logger.LogWarning("Kitap-Yazar ilişkisi eklenemedi: Geçersiz ID'ler. BookId: {BookId}, AuthorId: {AuthorId}", bookAuthor.BookId, bookAuthor.AuthorId);
                throw new ArgumentException("Geçerli Kitap veya Yazar ID'si belirtilmelidir.");
            }

            bool exists = await _bookRepository.IsBookAuthorExistsAsync(
                bookAuthor.BookId,
                bookAuthor.AuthorId
            );

            if (exists)
            {
                _logger.LogWarning("Kitap-Yazar ilişkisi zaten mevcut. BookId: {BookId}, AuthorId: {AuthorId}", bookAuthor.BookId, bookAuthor.AuthorId);
                throw new InvalidOperationException(
                    $"Kitap ID'si {bookAuthor.BookId} ve Yazar ID'si {bookAuthor.AuthorId} olan ilişki zaten mevcut."
                );
            }


            var added = await _bookRepository.AddBookAuthorAsync(bookAuthor);

            _logger.LogInformation("Kitap-Yazar ilişkisi kuruldu. BookId: {BookId}, AuthorId: {AuthorId}", bookAuthor.BookId, bookAuthor.AuthorId);

            return added;
        }
        public async Task<BookCopy> AddBookCopyAsync(CreateBookCopyDto createBookCopyDto)
        {
            _logger.LogInformation("Kitap kopyası ekleniyor. BookId: {BookId}, Barkod: {Barcode}", createBookCopyDto.BookId, createBookCopyDto.BarcodeNumber);

            var book = await _bookRepository.GetBookByIdAsync(createBookCopyDto.BookId);
            if (book == null)
            {
                _logger.LogWarning("Kitap kopyası eklenemedi: Ana kitap bulunamadı. BookId: {BookId}", createBookCopyDto.BookId);
                throw new KeyNotFoundException($"ID {createBookCopyDto.BookId} ile kayıtlı bir kitap bulunamadı.");
            }

            var room = await _roomService.GetRoomByIdAsync(createBookCopyDto.RoomId);
            if (room == null)
            {
                _logger.LogWarning("Kitap kopyası eklenemedi: Oda bulunamadı. RoomId: {RoomId}", createBookCopyDto.RoomId);
                throw new KeyNotFoundException($"ID {createBookCopyDto.RoomId} ile kayıtlı bir oda bulunamadı.");
            }

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

            var result = await _bookRepository.AddBookCopyAsync(bookCopy);

            _logger.LogInformation("Kitap kopyası başarıyla eklendi. CopyId: {CopyId}, Barkod: {Barcode}", result.Id, result.BarcodeNumber);

            return result;
        }



        public async Task<Book> UpdateBookAsync(int id, CreateBookDto updateBookDto)
        {
            if (updateBookDto == null)
                throw new ArgumentNullException(nameof(updateBookDto));

            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
            {
                _logger.LogWarning("Kitap güncelleme başarısız: Kitap bulunamadı. ID: {BookId}", id);
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap bulunamadı.");
            }

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

            _logger.LogInformation("Kitap bilgileri güncellendi. ID: {BookId}", id);

            return updatedBook;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
            {
                _logger.LogWarning("Kitap silme başarısız: Kitap bulunamadı. ID: {BookId}", id);
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap bulunamadı.");
            }

            var result = await _bookRepository.DeleteBookAsync(id);
            if (result)
            {
                _logger.LogInformation("Kitap silindi. ID: {BookId}", id);
            }
            return result;
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçerli bir kitap ID'si belirtilmelidir.", nameof(id));

            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning("Kitap sorgulama: ID {BookId} bulunamadı.", id);
            }
            return book;
        }

        public async Task<Book?> GetBookWithDetailsAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçerli bir kitap ID'si belirtilmelidir.", nameof(id));

            var book = await _bookRepository.GetBookWithDetailsAsync(id);
            if (book == null)
            {
                _logger.LogWarning("Detaylı kitap sorgulama: ID {BookId} bulunamadı.", id);
            }
            return book;
        }

        public async Task<PaginatedResult<Book>> GetAllBooksAsync(BookFilterDto filterDto)
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
            {
                _logger.LogWarning("Kitap kopyası güncelleme başarısız: Kopya bulunamadı. ID: {CopyId}", id);
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap kopyası bulunamadı.");
            }

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
                {
                    _logger.LogWarning("Kitap kopyası güncelleme başarısız: Yeni referans kitap bulunamadı. BookId: {BookId}", updateBookCopyDto.BookId.Value);
                    throw new KeyNotFoundException($"ID {updateBookCopyDto.BookId.Value} ile kayıtlı ana kitap bulunamadı.");
                }
            }

            var updatedCopy = await _bookRepository.UpdateBookCopyAsync(id, bookCopyToUpdate);

            _logger.LogInformation("Kitap kopyası güncellendi. ID: {CopyId}", id);

            return updatedCopy;
        }

        public async Task<bool> DeleteBookCopyAsync(int id)
        {
            var existingCopy = await _bookRepository.GetBookCopyByIdAsync(id);
            if (existingCopy == null)
            {
                _logger.LogWarning("Kitap kopyası silme başarısız: Kopya bulunamadı. ID: {CopyId}", id);
                throw new KeyNotFoundException($"ID {id} ile kayıtlı kitap kopyası bulunamadı.");
            }

            var result = await _bookRepository.DeleteBookCopyAsync(id);
            if (result)
            {
                _logger.LogInformation("Kitap kopyası silindi. ID: {CopyId}", id);
            }
            return result;
        }

        public async Task<BookCopy?> GetBookCopyByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("Barkod boş olamaz.", nameof(barcode));

            var bookCopy = await _bookRepository.GetBookCopyByBarcodeAsync(barcode);

            if (bookCopy == null)
            {
                _logger.LogWarning("Barkod ile sorgulama başarısız: {Barcode}", barcode);
                throw new KeyNotFoundException($"Barkod '{barcode}' ile eşleşen kitap bulunamadı.");
            }

            return bookCopy;
        }

        public async Task<bool> SetBookCopyUnAvailableAsync(int bookCopyId)
        {
            var result = await _bookRepository.SetBookCopyUnAvailableAsync(bookCopyId);
            if (result)
            {
                _logger.LogInformation("Kitap kopyası 'Müsait Değil' olarak işaretlendi. ID: {CopyId}", bookCopyId);
            }
            else
            {
                _logger.LogWarning("Kitap kopyası durumu değiştirilemedi. ID: {CopyId}", bookCopyId);
            }
            return result;
        }

        public async Task<IEnumerable<Book>> GetOtherBooksByAuthorAsync(int authorId, int? size, int? categoryId = null)
        {
            if (size <= 0)
            {
                _logger.LogWarning("Geçersiz size değeri: {Size}", size);
                throw new ArgumentException("Size 0'dan büyük olmalıdır.", nameof(size));
            }

            var authorExists = await _authorService.GetByIdAsync(authorId);

            if (authorExists == null)
            {
                _logger.LogWarning("Yazar bulunamadı. AuthorId: {AuthorId}", authorId);
                throw new KeyNotFoundException($"ID {authorId} ile kayıtlı yazar bulunamadı.");
            }

            var books = await _bookRepository.GetOtherBooksByAuthorAsync(authorId, size ?? 5,categoryId);

            return books;
        }

        public async Task<Book?> GetBookByNameWithDetailsAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Kitap ismi ile arama başarısız: Parametre boş veya null.");
                throw new ArgumentException("Arama yapılacak kitap ismi boş olamaz.", nameof(name));
            }

            _logger.LogInformation("Kitap ismi ile detaylı arama başlatıldı. Aranan: {Name}", name);

            var book = await _bookRepository.GetBookByNameWithDetailsAsync(name);

            if (book == null)
            {
                _logger.LogWarning("İsim ile kitap arama sonucu boş: '{Name}' ile eşleşen veya benzer bir kitap bulunamadı.", name);
            }
            else
            {
                _logger.LogInformation("Kitap ismi ile eşleşme bulundu. ID: {Id}, Başlık: {Title}", book.Id, book.Title);
            }

            return book;
        }
    }
}