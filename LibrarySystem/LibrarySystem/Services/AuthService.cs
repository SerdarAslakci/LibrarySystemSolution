using LibrarySystem.API.Dtos.AuthDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager; 
        private readonly SignInManager<AppUser> _signInManager; 
        public AuthService(ITokenService tokenService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public static string NormalizeUserName(string firstName, string lastName)
        {
            string fullName = (firstName + lastName).Replace(" ", "").ToLowerInvariant();

            fullName = fullName.Replace("ş", "s")
                               .Replace("ı", "i")
                               .Replace("ğ", "g")
                               .Replace("ç", "c")
                               .Replace("ü", "u")
                               .Replace("ö", "o");

            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

            return fullName + uniqueId;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
                throw new ArgumentException("E-posta adresi veya parola hatalı. Lütfen bilgilerinizi kontrol edin.");

            var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!checkPassword.Succeeded)
                throw new ArgumentException("E-posta adresi veya parola hatalı. Lütfen bilgilerinizi kontrol edin.");

            var refreshToken = await _tokenService.CreateRefreshTokenAsync();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            var token = await _tokenService.CreateTokenAsync(user);

            return new AuthResult
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                throw new ArgumentException("Geçersiz token isteği.");
            }

            if (user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                throw new ArgumentException("Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.");
            }

            var newAccessToken = await _tokenService.CreateTokenAsync(user);
            var newRefreshToken = await _tokenService.CreateRefreshTokenAsync();

            user.RefreshToken = newRefreshToken;

            await _userManager.UpdateAsync(user);

            return new AuthResult
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
        {
            var user = new AppUser
            {
                UserName = NormalizeUserName(registerDto.FirstName, registerDto.LastName),
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                DateOfBirth = registerDto.DateOfBirth,
            };

            var createResult = await _userManager.CreateAsync(user, registerDto.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();

                var userFriendlyMessage = "Kayıt işlemi başarısız.";

                if (errors.Any(e => e.Contains("is already taken")))
                {
                    userFriendlyMessage = "Bu e-posta adresi zaten sistemde kayıtlıdır.";
                }

                throw new InvalidOperationException(
                    userFriendlyMessage + " Detaylar: " + string.Join("; ", errors)
                );
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if(!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description).ToList();
                throw new InvalidOperationException("Kullanıcı rolü atama işlemi başarısız. Detaylar: " + string.Join("; ", errors));
            }

            var token = await _tokenService.CreateTokenAsync(user);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return new AuthResult
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = token,
                RefreshToken = refreshToken
            };
        }
    }
}
