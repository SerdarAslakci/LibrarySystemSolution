using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface ITokenService
    {
        public Task<string> CreateTokenAsync(AppUser user);
        public Task<string> CreateRefreshTokenAsync();
    }
}
