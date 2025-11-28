using LibrarySystem.API.Dtos.DashboardDtos;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}
