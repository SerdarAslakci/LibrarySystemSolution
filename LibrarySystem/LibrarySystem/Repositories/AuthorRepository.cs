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

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            return await _context.Authors
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .ToListAsync();
        }

        public async Task<bool> DeleteAuthorByIdAsync(int id)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .ThenInclude(ba => ba.Book)
                .ThenInclude(b => b.BookAuthors) 
                .FirstOrDefaultAsync(x => x.Id == id);

            if (author == null) return false;

            var booksToDelete = new List<Book>();

            foreach (var bookAuthor in author.BookAuthors)
            {
                var book = bookAuthor.Book;

                if (book.BookAuthors.Count == 1)
                {
                    booksToDelete.Add(book);
                }
            }

            if (booksToDelete.Any())
            {
                _context.Books.RemoveRange(booksToDelete);
            }

            _context.Authors.Remove(author);

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
