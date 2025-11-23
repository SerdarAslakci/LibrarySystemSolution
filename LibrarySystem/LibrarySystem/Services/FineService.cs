using AutoMapper;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class FineService : IFineService
    {
        private readonly IFineRepository _fineRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public FineService(IFineRepository fineRepository, IUserService userService, IMapper mapper)
        {
            _fineRepository = fineRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserFineDto>> GetUserFinesByEmailAsync(string email)
        {
            var user = await _userService.GetUserDetailByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"'{email}' email adresine sahip kullanıcı bulunamadı.");

            var fines = await _fineRepository.GetUserFinesWithLoanAsync(user.Id);

            return _mapper.Map<IEnumerable<UserFineDto>>(fines);
        }

        public async Task<UserFineDto?> PayFineAsync(int fineId)
        {
            var fine = await _fineRepository.PayFineByIdAscyn(fineId);

            if (fine == null) 
                throw new KeyNotFoundException("Ceza bulunamadı.");

            var userFineDto = _mapper.Map<UserFineDto>(fine);

            return userFineDto;
        }

        public async Task<Fine?> ProcessLateReturnAsync(Loan loan)
        {
            if (loan == null)
            {
                throw new ArgumentNullException(nameof(loan), "Ceza hesaplama işlemi için gönderilen ödünç (Loan) kaydı boş olamaz.");
            }

            if (loan.ActualReturnDate == null)
            {
                throw new InvalidOperationException("Kitap henüz iade edilmediği (ActualReturnDate boş olduğu) için ceza hesaplanamaz.");
            }

            if (string.IsNullOrEmpty(loan.UserId) || loan.Id <= 0)
            {
                throw new ArgumentException("Geçersiz Loan veya User ID bilgisi.");
            }

            try
            {
                var fine = await _fineRepository.ProcessLateReturnAsync(loan);

                return fine;
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Ceza işlemi sırasında beklenmeyen bir hata oluştu. Lütfen sistem yöneticisine başvurun.", ex);
            }
        }
    }
}
