using LibrarySystem.API.DataContext;
using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly AppDbContext _context;

        public AuthorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Author> AddAuthorAsync(Author author)
        {
            var added = await _context.Authors.AddAsync(author);

            await _context.SaveChangesAsync();

            return added.Entity;
        }

        public async Task<bool> IsExistsAsync(string? firstName, string? lastName)
        {
            return await _context.Authors
                .AnyAsync(a =>
                    a.FirstName != null && a.LastName != null &&
                    a.FirstName.ToLower() == (firstName ?? string.Empty).ToLower() &&
                    a.LastName.ToLower() == (lastName ?? string.Empty).ToLower());
        }

        public async Task<Author?> GetByIdAsync(int id)
        {
            return await _context.Authors.FindAsync(id);
        }

        public async Task<Author?> GetByNameAsync(string firstName, string lastName)
        {
            return await _context.Authors
                .FirstOrDefaultAsync(a =>
                    a.FirstName.ToLower() == firstName.ToLower() &&
                    a.LastName.ToLower() == lastName.ToLower());
        }
    }
}
