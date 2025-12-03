using LibrarySystem.API.DataContext;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class FineRepository : IFineRepository
    {
        private readonly AppDbContext _context;
        public FineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Fine> AddFineAsync(Fine fine)
        {
            var entity = await _context.Fines.AddAsync(fine);

            await _context.SaveChangesAsync();

            return entity.Entity;
        }

        public async Task<IEnumerable<Fine>> GetUserFinesWithLoanAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "User ID boş olamaz.");

            return await _context.Fines
                .Include(f => f.Loan)
                    .ThenInclude(l => l.BookCopy)
                        .ThenInclude(bc => bc.Book)
                .Include(f => f.FineType)
                .Where(f => f.UserId == userId && f.IsActive == true)
                .OrderByDescending(f => f.IssuedDate)
                .ToListAsync();
        }

        public async Task<Fine?> RevokeFineByIdAscyn(int id)
        {
            var fine = await _context.Fines
                .Include(f => f.Loan)
                    .ThenInclude(l => l.BookCopy)
                        .ThenInclude(bc => bc.Book)
                .Include(f => f.FineType)
                .FirstOrDefaultAsync(f => f.Id == id);


            if (fine == null)
            {
                return null;
            }
            fine.Status = "Paid";
            fine.IsActive = false;

            await _context.SaveChangesAsync();  

            return fine;
        }

        public async Task<Fine?> ProcessLateReturnAsync(Loan loan)
        {
            if (loan.ActualReturnDate == null)
            {
                return null;
            }

            if (loan.ActualReturnDate <= loan.ExpectedReturnDate)
            {
                return null;
            }

            TimeSpan delay = loan.ActualReturnDate.Value - loan.ExpectedReturnDate;
            int overdueDays = (int)Math.Ceiling(delay.TotalDays);

            if (overdueDays <= 0) return null;

            var fineType = await _context.FineTypes.FirstOrDefaultAsync(x => x.Name == "Gecikme");

            if (fineType == null)
            {
                throw new InvalidOperationException("Sistemde 'Gecikme' (ID:1) ceza tipi tanımlı değil.");
            }

            var fine = new Fine
            {
                UserId = loan.UserId,
                LoanId = loan.Id,
                FineTypeId = fineType.Id,
                Amount = overdueDays * fineType.DailyRate,
                Status = "Unpaid",
                IsActive = true,
                IssuedDate = DateTime.Now,
                Description = $"{overdueDays} gün gecikme nedeniyle ceza."
            };

            await _context.Fines.AddAsync(fine);
            await _context.SaveChangesAsync();

            return fine;
        }

        public async Task<IEnumerable<Fine>> GetActiveFinesByUserIdAsync(string userId, int page, int pageSize)
        {
            return await _context.Fines
                .Include(f => f.Loan)
                    .ThenInclude(l => l.BookCopy)
                        .ThenInclude(bc => bc.Book)
                .Include(f => f.FineType)
                .Where(f => f.UserId == userId && f.IsActive == true)
                .OrderByDescending(f => f.IssuedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fine>> GetInActiveFinesByUserIdAsync(string userId, int page, int pageSize)
        {
            return await _context.Fines
                .Include(f => f.Loan)
                    .ThenInclude(l => l.BookCopy)
                        .ThenInclude(bc => bc.Book)
                .Include(f => f.FineType)
                .Where(f => f.UserId == userId && f.IsActive == false)
                .OrderByDescending(f => f.IssuedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
