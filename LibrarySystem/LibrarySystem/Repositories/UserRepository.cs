using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await _userManager.GetUsersInRoleAsync("User");
        }
        public async Task<IEnumerable<AppUser>> GetUsersInRoleAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
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
