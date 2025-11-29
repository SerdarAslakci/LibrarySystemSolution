using AutoMapper;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<UserViewDto>> GetUsersForListingAsync(string? roleFilter = null)
        {
            IEnumerable<UserViewDto> users;

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                users = await _userRepository.GetUsersInRoleAsync(roleFilter);

                if (!users.Any())
                {
                    _logger.LogWarning("Kullanıcı listeleme uyarısı: '{Role}' rolüne ait kullanıcı bulunamadı.", roleFilter);
                    throw new KeyNotFoundException($"'{roleFilter}' rolüne ait kullanıcı bulunamadı.");
                }
            }
            else
            {
                users = await _userRepository.GetAllUsersAsync();
            }

            return users;
        }

        public async Task<UserViewDto?> GetUserDetailByIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Kullanıcı detayı sorgulama başarısız: ID '{UserId}' bulunamadı.", userId);
                throw new KeyNotFoundException($"ID değeri '{userId}' olan kullanıcı bulunamadı.");
            }

            return user;
        }

        public async Task<UserViewDto?> GetUserDetailByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Kullanıcı detayı sorgulama başarısız: Email '{Email}' bulunamadı.", email);
                throw new KeyNotFoundException($"'{email}' email adresine sahip kullanıcı bulunamadı.");
            }

            return user;
        }
    }
}