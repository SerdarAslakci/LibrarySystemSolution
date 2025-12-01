using AutoMapper;
using LibrarySystem.API.Dtos.FineTypeDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class FineTypeService : IFineTypeService
    {

        private readonly IFineTypeRepository _fineTypeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FineTypeService> _logger;

        public FineTypeService(IFineTypeRepository fineTypeRepository, IMapper mapper, ILogger<FineTypeService> logger)
        {
            _fineTypeRepository = fineTypeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReturnFineTypeDto> AddFineTypeAsync(CreateFineTypeDto fineType)
        {
            _logger.LogInformation("Yeni ceza tipi ekleme isteği: {Name}, Tutar: {DailyRate}", fineType?.Name, fineType?.DailyRate);

            if (fineType == null)
            {
                _logger.LogWarning("Ceza tipi ekleme başarısız: DTO boş.");
                throw new ArgumentNullException(nameof(fineType), "Ceza tipi boş olamaz.");
            }

            if (string.IsNullOrWhiteSpace(fineType.Name))
            {
                _logger.LogWarning("Ceza tipi ekleme başarısız: İsim boş.");
                throw new ArgumentException("Ceza tipi adı boş olamaz.", nameof(fineType));
            }

            if (fineType.DailyRate < 0)
            {
                _logger.LogWarning("Ceza tipi ekleme başarısız: Geçersiz tutar ({DailyRate}).", fineType.DailyRate);
                throw new ArgumentException("Günlük ceza tutarı 0 veya negatif olamaz.", nameof(fineType));
            }


            var entity = new FineType
            {
                Name = fineType.Name,
                DailyRate = fineType.DailyRate
            };

            var added = await _fineTypeRepository.AddFineTypeAsync(entity);

            _logger.LogInformation("Ceza tipi başarıyla eklendi. ID: {Id}", added.Id);

            return _mapper.Map<ReturnFineTypeDto>(added);
        }

        public async Task<ReturnFineTypeDto> UpdateFineTypeAsync(UpdateFineTypeDto fineType)
        {
            if (fineType == null)
                throw new ArgumentNullException(nameof(fineType), "Ceza tipi boş olamaz.");

            var existingFineType = await _fineTypeRepository.GetByIdAsync(fineType.Id);
            if (existingFineType == null)
            {
                _logger.LogWarning("Ceza tipi güncelleme başarısız: ID {Id} bulunamadı.", fineType.Id);
                throw new KeyNotFoundException($"Id'si {fineType.Id} olan ceza tipi bulunamadı.");
            }

            existingFineType.Name = fineType.Name ?? existingFineType.Name;
            existingFineType.DailyRate = fineType.DailyRate > 0 ? fineType.DailyRate : existingFineType.DailyRate;

            var updated = await _fineTypeRepository.UpdateFineTypeAsync(existingFineType);

            _logger.LogInformation("Ceza tipi güncellendi. ID: {Id}", updated.Id);

            return _mapper.Map<ReturnFineTypeDto>(updated);
        }

        public async Task<ReturnFineTypeDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçersiz Id değeri.", nameof(id));

            var fineType = await _fineTypeRepository.GetByIdAsync(id);
            if (fineType == null)
            {
                _logger.LogWarning("Ceza tipi sorgulama başarısız: ID {Id} bulunamadı.", id);
                throw new KeyNotFoundException($"Id'si {id} olan ceza tipi bulunamadı.");
            }

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