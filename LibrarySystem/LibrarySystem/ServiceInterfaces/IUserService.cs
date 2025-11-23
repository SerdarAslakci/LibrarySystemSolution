using LibrarySystem.API.Dtos.UserDtos;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserViewDto>> GetUsersForListingAsync(string? roleFilter = null);
        Task<UserViewDto?> GetUserDetailByIdAsync(string userId);
        Task<UserViewDto?> GetUserDetailByEmailAsync(string email);
    }
}
