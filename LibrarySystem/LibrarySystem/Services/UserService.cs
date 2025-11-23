using AutoMapper;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserViewDto>> GetUsersForListingAsync(string? roleFilter = null)
        {
            IEnumerable<AppUser> users;

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                users = await _userRepository.GetUsersInRoleAsync(roleFilter);

                if (!users.Any())
                    throw new KeyNotFoundException($"'{roleFilter}' rolüne ait kullanıcı bulunamadı.");
            }
            else
            {
                users = await _userRepository.GetAllUsersAsync();
            }

            return _mapper.Map<IEnumerable<UserViewDto>>(users);
        }

        public async Task<UserViewDto?> GetUserDetailByIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException($"ID değeri '{userId}' olan kullanıcı bulunamadı.");

            return _mapper.Map<UserViewDto>(user);
        }

        public async Task<UserViewDto?> GetUserDetailByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
                throw new KeyNotFoundException($"'{email}' email adresine sahip kullanıcı bulunamadı.");

            return _mapper.Map<UserViewDto>(user);
        }
    }
}
