using LibrarySystem.API.DataContext;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UserRepository(UserManager<AppUser> userManager,AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<PaginatedResult<UserViewDto>> GetUsersWithFilterAsync(UserFilterDto filter)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FirstName))
                query = query.Where(u => u.FirstName.Contains(filter.FirstName));

            if (!string.IsNullOrWhiteSpace(filter.LastName))
                query = query.Where(u => u.LastName.Contains(filter.LastName));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(u => u.Email.Contains(filter.Email));

            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                query = query.Where(u => _context.UserRoles.Any(ur =>
                    ur.UserId == u.Id &&
                    _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == filter.Role)));
            }

            if (filter.HasFine.HasValue)
            {
                if (filter.HasFine.Value == true)
                    query = query.Where(u => _context.Fines.Any(f => f.UserId == u.Id && f.IsActive));
                else                
                    query = query.Where(u => !_context.Fines.Any(f => f.UserId == u.Id && f.IsActive));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(user => new UserViewDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    PhoneNumber = user.PhoneNumber,
                    Roles = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == user.Id
                             select r.Name).ToList(),
                    loanBookCount = _context.Loans.Count(loan => loan.UserId == user.Id),
                    HasFine = _context.Fines.Any(fine => fine.UserId == user.Id && fine.IsActive)
                })
                .ToListAsync();

            return new PaginatedResult<UserViewDto>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<UserViewDto?> GetUserByEmailAsync(string email)
        {
            var query = _context.Users
                .Where(user => user.Email == email)
                .Select(user => new UserViewDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    PhoneNumber = user.PhoneNumber,
                    Roles = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == user.Id
                             select r.Name).ToList(),
                    loanBookCount = _context.Loans.Count(loan => loan.UserId == user.Id),
                    HasFine = _context.Fines.Any(fine => fine.UserId == user.Id && fine.IsActive)
                });

            return await query.FirstOrDefaultAsync();
        }

        public async Task<UserViewDto?> GetUserByIdAsync(string userId)
        {
            var query = _context.Users
                .Where(user => user.Id == userId) 
                .Select(user => new UserViewDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    PhoneNumber = user.PhoneNumber,
                    Roles = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == user.Id
                             select r.Name).ToList(),
                    loanBookCount = _context.Loans.Count(loan => loan.UserId == user.Id),
                    HasFine = _context.Fines.Any(fine => fine.UserId == user.Id && fine.IsActive)
                });

            return await query.FirstOrDefaultAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");
            return users.Count;
        }

        public async Task<UserStatsDto> GetUserStatsAsync(string userId)
        {
            var totalActiveLoans = await _context.Loans.CountAsync(loan => loan.UserId == userId && loan.ActualReturnDate == null);
            var totalCompletedLoans = await _context.Loans.CountAsync(loan => loan.UserId == userId && loan.ActualReturnDate != null);
            var totalFines = await _context.Fines.Where(fine => fine.UserId == userId && fine.IsActive == true).SumAsync(fine => fine.Amount);


            return new UserStatsDto
            {
                ActiveLoanCount = totalActiveLoans,
                TotalReadCount = totalCompletedLoans,
                TotalFineDebt = totalFines
            };
        }
    }
}
