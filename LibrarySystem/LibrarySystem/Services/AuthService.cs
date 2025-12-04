using LibrarySystem.API.Dtos.AuthDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ITokenService tokenService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AuthService> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public static string NormalizeUserName(string firstName, string lastName)
        {
            string rawText = (firstName + lastName).Replace(" ", "");
            string text = rawText
                .Replace("İ", "i").Replace("I", "i").Replace("ı", "i")
                .Replace("Ğ", "g").Replace("ğ", "g")
                .Replace("Ü", "u").Replace("ü", "u")
                .Replace("Ş", "s").Replace("ş", "s")
                .Replace("Ö", "o").Replace("ö", "o")
                .Replace("Ç", "c").Replace("ç", "c");

            string normalized = text.ToLowerInvariant();

            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[^a-z0-9]", "");

            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

            return normalized + uniqueId;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Kullanıcı giriş denemesi: {Email}", loginDto.Email);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !user.Email.Equals(loginDto.Email, StringComparison.Ordinal))
            {
                _logger.LogWarning("Başarısız giriş denemesi: E-posta bulunamadı veya case-sensitive uyuşmuyor. ({Email})", loginDto.Email);
                throw new ArgumentException("E-posta adresi veya parola hatalı. Lütfen bilgilerinizi kontrol edin.");
            }


            if (user == null)
            {
                _logger.LogWarning("Başarısız giriş denemesi: E-posta bulunamadı. ({Email})", loginDto.Email);
                throw new ArgumentException("E-posta adresi veya parola hatalı. Lütfen bilgilerinizi kontrol edin.");
            }

            var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!checkPassword.Succeeded)
            {
                _logger.LogWarning("Başarısız giriş denemesi: Yanlış parola. Kullanıcı: {Email}", loginDto.Email);
                throw new ArgumentException("E-posta adresi veya parola hatalı. Lütfen bilgilerinizi kontrol edin.");
            }

            var refreshToken = await _tokenService.CreateRefreshTokenAsync();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            var token = await _tokenService.CreateTokenAsync(user);

            _logger.LogInformation("Kullanıcı başarıyla giriş yaptı: {UserName} ({Email})", user.UserName, user.Email);

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
                _logger.LogWarning("Token yenileme başarısız: Geçersiz RefreshToken kullanıldı.");
                throw new ArgumentException("Geçersiz token isteği.");
            }

            if (user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                _logger.LogWarning("Token yenileme başarısız: RefreshToken süresi dolmuş. Kullanıcı: {UserName}", user.UserName);
                throw new ArgumentException("Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.");
            }

            var newAccessToken = await _tokenService.CreateTokenAsync(user);
            var newRefreshToken = await _tokenService.CreateRefreshTokenAsync();

            user.RefreshToken = newRefreshToken;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token başarıyla yenilendi. Kullanıcı: {UserName}", user.UserName);

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
            _logger.LogInformation("Yeni kullanıcı kayıt isteği: {Email}", registerDto.Email);

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
                string errorString = string.Join("; ", errors);

                _logger.LogWarning("Kullanıcı oluşturma başarısız ({Email}). Hatalar: {Errors}", registerDto.Email, errorString);

                var userFriendlyMessage = "Kayıt işlemi başarısız.";

                if (errors.Any(e => e.Contains("is already taken")))
                {
                    userFriendlyMessage = "Bu e-posta adresi zaten sistemde kayıtlıdır.";
                }

                throw new InvalidOperationException(
                    userFriendlyMessage + " Detaylar: " + errorString
                );
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description).ToList();
                string errorString = string.Join("; ", errors);

                _logger.LogError("Kullanıcı oluşturuldu ancak ROL atanamadı! Kullanıcı: {UserName}, Hatalar: {Errors}", user.UserName, errorString);

                throw new InvalidOperationException("Kullanıcı rolü atama işlemi başarısız. Detaylar: " + errorString);
            }

            var token = await _tokenService.CreateTokenAsync(user);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Yeni kullanıcı başarıyla kaydedildi: {UserName} ({Email})", user.UserName, user.Email);

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