using LibrarySystem.API.DataContext;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public LoanRepository(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Loan> AddLoanAsync(Loan loan)
        {
            var added = await _context.Loans.AddAsync(loan);
            await _context.SaveChangesAsync();

            var addedLoanWithDetails = await _context.Loans
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(l => l.BookCopy.Shelf)
                    .ThenInclude(s => s.Room)
                .Include(l => l.AppUser)
                .FirstOrDefaultAsync(l => l.Id == loan.Id);

            return addedLoanWithDetails;
        }

        public async Task<bool> CanUserBarrowAsync(string userId)
        {
            return !await _context.Fines
                .AnyAsync(x => x.UserId == userId && x.IsActive == true);
        }

        public async Task<Loan?> MarkAsReturnedByBarcodeAsync(string barcode)
        {
            var loan = await _context.Loans
                .Include(l => l.BookCopy)
                .Include(l => l.BookCopy.Book)
                .Include(l => l.AppUser)
                .FirstOrDefaultAsync(l =>
                    l.BookCopy.BarcodeNumber == barcode &&
                    l.ActualReturnDate == null);

            if (loan == null)
            {
                return null;
            }

            loan.ActualReturnDate = DateTime.Now;
            loan.BookCopy.IsAvailable = true;

            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task<Loan?> GetLoanByIdAsync(int id)
        {
            var loan = await _context.Loans
             .Include(l => l.BookCopy)
                 .ThenInclude(bc => bc.Book)
                     .ThenInclude(b => b.BookAuthors)
                         .ThenInclude(ba => ba.Author)
             .Include(l => l.BookCopy.Shelf)
                 .ThenInclude(s => s.Room)
             .FirstOrDefaultAsync(l => l.Id == id);

            return loan;
        }

        public async Task<bool> IsBookCopyOnLoanAsync(int bookCopyId)
        {
            return await _context.Loans
                .AnyAsync(x => x.BookCopyId == bookCopyId && x.ActualReturnDate == null);
        }

        public async Task<Loan?> UpdateLoanAsync(Loan loan)
        {
            var updated  = _context.Loans.Update(loan);
            await _context.SaveChangesAsync();

            return await GetLoanByIdAsync(loan.Id);
        }

        public async Task<IEnumerable<Loan?>> GetAllLoansByUserAsync(string userId)
        {
            var loans = await _context.Loans
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                .Include(l => l.BookCopy.Shelf)
                    .ThenInclude(s => s.Room)
                .Where(l => l.UserId == userId)
                .ToListAsync();

            return loans;
        }
    }
}
