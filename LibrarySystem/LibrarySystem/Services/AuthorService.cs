using AutoMapper;
using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public AuthorService(IAuthorRepository authorRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        public async Task<Author> AddAuthorAsync(CreateAuthorDto authorDto)
        {
            if (authorDto == null)
                throw new ArgumentNullException(nameof(authorDto), "Yazar bilgisi boş olamaz.");

            if (string.IsNullOrWhiteSpace(authorDto.FirstName) || string.IsNullOrWhiteSpace(authorDto.LastName))
                throw new ArgumentException("Yazarın adı ve soyadı boş bırakılamaz.");

            bool exists = await _authorRepository.IsExistsAsync(authorDto.FirstName, authorDto.LastName);
            if (exists)
                throw new InvalidOperationException("Bu yazar zaten sistemde mevcut.");

            var author = _mapper.Map<Author>(authorDto);

            await _authorRepository.AddAuthorAsync(author);

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
                throw new KeyNotFoundException($"ID'si {id} olan yazar bulunamadı.");

            return author;
        }

        public async Task<Author?> GetByNameAsync(string firstName, string lastName)
        {
            var author = await _authorRepository.GetByNameAsync(firstName, lastName);
            if (author == null)
                throw new KeyNotFoundException($"Adı '{firstName} {lastName}' olan yazar bulunamadı.");

            return author;
        }

        public async Task<Author> GetOrCreateAsync(int? id, string? firstName, string? lastName)
        {
            if (id.HasValue)
            {
                return await GetByIdAsync(id.Value);
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Yazar adı ve soyadı boş olamaz.");

            try
            {
                return await GetByNameAsync(firstName, lastName);
            }
            catch (KeyNotFoundException)
            {
                return await AddAuthorAsync(new CreateAuthorDto
                {
                    FirstName = firstName,
                    LastName = lastName
                });
            }
        }

    }

}
