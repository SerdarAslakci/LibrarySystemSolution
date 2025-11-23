using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
        Task<IEnumerable<AppUser>> GetUsersInRoleAsync(string roleName);
        Task<AppUser?> GetUserByIdAsync(string userId);
        Task<AppUser?> GetUserByEmailAsync(string email);
    }
}
