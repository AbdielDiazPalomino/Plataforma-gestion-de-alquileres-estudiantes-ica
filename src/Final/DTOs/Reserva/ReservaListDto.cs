namespace Final.DTOs.Reserva;

public class ReservaListDto
{
    public int Id { get; set; }
    public string PropiedadTitulo { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    public decimal PrecioTotal { get; set; }
    public string Estado { get; set; }

    // Cliente (usuario que hizo la reserva)
    public string UsuarioNombre { get; set; }
    public string UsuarioEmail { get; set; }
}
