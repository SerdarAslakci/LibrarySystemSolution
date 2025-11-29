using LibrarySystem.API.DataContext;
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

        public async Task<IEnumerable<UserViewDto>> GetAllUsersAsync()
        {
            var query = _context.Users.Select(user => new UserViewDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = (from ur in _context.UserRoles
                         join r in _context.Roles on ur.RoleId equals r.Id
                         where ur.UserId == user.Id
                         select r.Name).ToList(),
                loanBookCount = _context.Loans.Count(loan => loan.UserId == user.Id),
                HasFine = _context.Fines.Any(fine => fine.UserId == user.Id && fine.IsActive)
            });

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<UserViewDto>> GetUsersInRoleAsync(string roleName)
        {
            var query = _context.Users
                .Where(user => _context.UserRoles.Any(ur =>
                    ur.UserId == user.Id &&
                    _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == roleName)
                ))
                .Select(user => new UserViewDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == user.Id
                             select r.Name).ToList(),
                    loanBookCount = _context.Loans.Count(loan => loan.UserId == user.Id),
                    HasFine = _context.Fines.Any(fine => fine.UserId == user.Id && fine.IsActive)
                });

            return await query.ToListAsync();
        }

        public async Task<AppUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<AppUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<int> GetUserCountAsync()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");
            return users.Count;
        }
    }
}
