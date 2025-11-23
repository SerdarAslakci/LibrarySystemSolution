using LibrarySystem.API.Dtos.AuthDtos;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto registerDto);
        Task<AuthResult> LoginAsync(LoginDto loginDto);
        Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
    }
}
