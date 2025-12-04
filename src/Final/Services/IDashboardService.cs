using Final.DTOs.Dashboard;

namespace Final.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
        Task<List<PropiedadStatsDto>> GetTopPropiedadesAsync();
    }
}