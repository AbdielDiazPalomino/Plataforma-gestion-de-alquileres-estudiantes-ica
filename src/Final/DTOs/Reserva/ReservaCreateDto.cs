namespace Final.DTOs.Reserva;

public class ReservaCreateDto
{
    public int PropiedadId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
}
