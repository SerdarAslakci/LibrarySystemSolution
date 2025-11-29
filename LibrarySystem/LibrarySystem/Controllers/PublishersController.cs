using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublishersController : ControllerBase
    {
        private readonly IPublisherService _publisherService;
        private readonly ILogger<PublishersController> _logger;

        public PublishersController(IPublisherService publisherService, ILogger<PublishersController> logger)
        {
            _publisherService = publisherService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Yayınevi listesi getiriliyor...");

            var publishers = await _publisherService.GetAllAsync();

            _logger.LogInformation("Toplam {Count} yayınevi gönderiliyor.", publishers.Count());

            return Ok(publishers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("ID {Id} olan yayınevi isteniyor.", id);

            try
            {
                var publisher = await _publisherService.GetByIdAsync(id);

                _logger.LogInformation("ID {Id} için yayınevi bulundu: {Name}", id, publisher.Name);

                return Ok(publisher);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("ID {Id} için yayınevi bulunamadı.", id);
                return NotFound(ex.Message);
            }
        }
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            _logger.LogInformation("'{Name}' adlı yayınevi isteniyor.", name);

            try
            {
                var publisher = await _publisherService.GetByNameAsync(name);

                _logger.LogInformation("'{Name}' adlı yayınevi bulundu.", name);

                return Ok(publisher);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("'{Name}' adlı yayınevi bulunamadı.", name);
                return NotFound(ex.Message);
            }
        }
    }

}
