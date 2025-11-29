using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserViewDto>> GetAllUsersAsync();
        Task<IEnumerable<UserViewDto>> GetUsersInRoleAsync(string roleName);
        Task<UserViewDto?> GetUserByIdAsync(string userId);
        Task<UserViewDto?> GetUserByEmailAsync(string email);
        Task<int> GetUserCountAsync();
    }
}
