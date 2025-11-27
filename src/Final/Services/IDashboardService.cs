// Services/IDashboardService.cs
namespace Final.Services
{
    public interface IDashboardService
    {
        Task<DashboardStats> GetStatsAsync();
        Task<List<PropiedadStats>> GetTopPropiedadesAsync();
    }

    public class DashboardStats
    {
        public int TotalUsuarios { get; set; }
        public int TotalPropiedades { get; set; }
        public int TotalReservas { get; set; }
        public int PropiedadesPendientes { get; set; }
        public decimal IngresosTotales { get; set; }
    }

    public class PropiedadStats
    {
        public int PropiedadId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int TotalReservas { get; set; }
        public double CalificacionPromedio { get; set; }
    }
}