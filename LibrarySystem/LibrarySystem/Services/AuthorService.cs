using AutoMapper;
using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorService> _logger;

        public AuthorService(IAuthorRepository authorRepository, IMapper mapper, ILogger<AuthorService> logger)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Author> AddAuthorAsync(CreateAuthorDto authorDto)
        {
            _logger.LogInformation("Yazar ekleme işlemi başlatıldı. İsim: {FirstName} {LastName}", authorDto?.FirstName, authorDto?.LastName);

            if (authorDto == null)
            {
                _logger.LogWarning("Yazar ekleme başarısız: Yazar bilgisi (Dto) null geldi.");
                throw new ArgumentNullException(nameof(authorDto), "Yazar bilgisi boş olamaz.");
            }

            if (string.IsNullOrWhiteSpace(authorDto.FirstName) || string.IsNullOrWhiteSpace(authorDto.LastName))
            {
                _logger.LogWarning("Yazar ekleme başarısız: Ad veya soyad boş geçilmiş.");
                throw new ArgumentException("Yazarın adı ve soyadı boş bırakılamaz.");
            }

            bool exists = await _authorRepository.IsExistsAsync(authorDto.FirstName, authorDto.LastName);
            if (exists)
            {
                _logger.LogWarning("Yazar ekleme başarısız: '{FirstName} {LastName}' zaten sistemde mevcut.", authorDto.FirstName, authorDto.LastName);
                throw new InvalidOperationException("Bu yazar zaten sistemde mevcut.");
            }

            var author = _mapper.Map<Author>(authorDto);

            await _authorRepository.AddAuthorAsync(author);

            _logger.LogInformation("Yazar başarıyla eklendi. Yeni ID: {AuthorId}", author.Id);

            return author;
        }

        public async Task<bool> IsExistsAsync(string? firstName, string? lastName)
        {
            return await _authorRepository.IsExistsAsync(firstName, lastName);
        }

        public async Task<Author?> GetByIdAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                _logger.LogWarning("Yazar sorgulama başarısız: ID'si {Id} olan yazar bulunamadı.", id);
                throw new KeyNotFoundException($"ID'si {id} olan yazar bulunamadı.");
            }

            return author;
        }

        public async Task<Author?> GetByNameAsync(string firstName, string lastName)
        {
            var author = await _authorRepository.GetByNameAsync(firstName, lastName);
            if (author == null)
            {
                _logger.LogWarning("Yazar sorgulama başarısız: '{FirstName} {LastName}' isimli yazar bulunamadı.", firstName, lastName);
                throw new KeyNotFoundException($"Adı '{firstName} {lastName}' olan yazar bulunamadı.");
            }

            return author;
        }

        public async Task<Author> GetOrCreateAsync(int? id, string? firstName, string? lastName)
        {
            if (id.HasValue)
            {
                return await GetByIdAsync(id.Value);
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                _logger.LogWarning("GetOrCreate başarısız: İsim parametreleri eksik.");
                throw new ArgumentException("Yazar adı ve soyadı boş olamaz.");
            }

            try
            {
                return await GetByNameAsync(firstName, lastName);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation("GetOrCreate: '{FirstName} {LastName}' bulunamadı, otomatik olarak yeni kayıt oluşturuluyor.", firstName, lastName);

                return await AddAuthorAsync(new CreateAuthorDto
                {
                    FirstName = firstName,
                    LastName = lastName
                });
            }
        }

        public AuthorService(IAuthorRepository authorRepository, ILogger<AuthorService> logger)
        {
            _authorRepository = authorRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            _logger.LogInformation("Service: Tüm yazarları getirme işlemi çağrıldı.");

            return await _authorRepository.GetAllAuthorsAsync();
        }
    }
}