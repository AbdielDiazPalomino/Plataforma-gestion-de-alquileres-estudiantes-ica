namespace Final.DTOs.Dashboard
{
    public class PropiedadStatsDto
    {
        public int PropiedadId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int TotalReservas { get; set; }
        public double CalificacionPromedio { get; set; }
    }
}