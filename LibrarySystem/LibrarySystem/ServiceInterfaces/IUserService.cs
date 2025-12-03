using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.UserDtos;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IUserService
    {
        Task<UserStatsDto> GetUserStatsAsync(string userId);
        Task<PaginatedResult<UserViewDto>> GetUsersForListingAsync(UserFilterDto filter);
        Task<UserViewDto?> GetUserDetailByIdAsync(string userId);
        Task<UserViewDto?> GetUserDetailByEmailAsync(string email);
    }
}
