using AutoMapper;
using LibrarySystem.API.Dtos.FineTypeDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class FineTypeService : IFineTypeService
    {

        private readonly IFineTypeRepository _fineTypeRepository;
        private readonly IMapper _mapper;

        public FineTypeService(IFineTypeRepository fineTypeRepository,IMapper mapper)
        {
            _fineTypeRepository = fineTypeRepository;
            _mapper = mapper;
        }

        public async Task<ReturnFineTypeDto> AddFineTypeAsync(CreateFineTypeDto fineType)
        {
            if (fineType == null)
                throw new ArgumentNullException(nameof(fineType), "Ceza tipi boş olamaz.");

            if (string.IsNullOrWhiteSpace(fineType.Name))
                throw new ArgumentException("Ceza tipi adı boş olamaz.", nameof(fineType));

            if (fineType.DailyRate <= 0)
                throw new ArgumentException("Günlük ceza tutarı 0 veya negatif olamaz.", nameof(fineType));


            var entity = new FineType
            {
                Name = fineType.Name,
                DailyRate = fineType.DailyRate
            };

            var added = await _fineTypeRepository.AddFineTypeAsync(entity);

            return _mapper.Map<ReturnFineTypeDto>(added);
        }

        public async Task<ReturnFineTypeDto> UpdateFineTypeAsync(UpdateFineTypeDto fineType)
        {
            if (fineType == null)
                throw new ArgumentNullException(nameof(fineType), "Ceza tipi boş olamaz.");

            var existingFineType = await _fineTypeRepository.GetByIdAsync(fineType.Id);
            if (existingFineType == null)
                throw new KeyNotFoundException($"Id'si {fineType.Id} olan ceza tipi bulunamadı.");

            existingFineType.Name = fineType.Name ?? existingFineType.Name;
            existingFineType.DailyRate = fineType.DailyRate > 0 ? fineType.DailyRate : existingFineType.DailyRate;

            var updated = await _fineTypeRepository.UpdateFineTypeAsync(existingFineType);

            return _mapper.Map<ReturnFineTypeDto>(updated);
        }

        public async Task<ReturnFineTypeDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçersiz Id değeri.", nameof(id));

            var fineType = await _fineTypeRepository.GetByIdAsync(id);
            if (fineType == null)
                throw new KeyNotFoundException($"Id'si {id} olan ceza tipi bulunamadı.");

            return _mapper.Map<ReturnFineTypeDto>(fineType);
        }
        public async Task<List<ReturnFineTypeDto>> GetAllFineTypesAsync()
        {
            var fineTypes = await _fineTypeRepository.GetAllFineTypesAsync();
            if (fineTypes == null || fineTypes.Count == 0)
                return new List<ReturnFineTypeDto>();

            var fineTypeDtos = _mapper.Map<List<ReturnFineTypeDto>>(fineTypes);

            return fineTypeDtos;
        }
    }
}
