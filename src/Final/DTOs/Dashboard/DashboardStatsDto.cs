namespace Final.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalUsuarios { get; set; }
        public int TotalPropiedades { get; set; }
        public int TotalReservas { get; set; }
        public int PropiedadesPendientes { get; set; }
        public decimal IngresosTotales { get; set; }
    }
}