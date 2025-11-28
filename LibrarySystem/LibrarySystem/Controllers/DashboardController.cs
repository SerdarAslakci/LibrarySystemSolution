using LibrarySystem.API.Dtos.DashboardDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Dashboard API çağrısı başladı.");

                var dashboardData = await _dashboardService.GetDashboardDataAsync();

                _logger.LogInformation("Dashboard API çağrısı başarıyla tamamlandı.");

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard API çağrısı sırasında bir hata oluştu.");
                return StatusCode(500, "Dashboard verileri alınırken bir hata oluştu.");
            }
        }
    }
}
