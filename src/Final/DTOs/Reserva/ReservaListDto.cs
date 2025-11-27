namespace Final.DTOs.Reserva;

public class ReservaListDto
{
    public int Id { get; set; }
    public string PropiedadTitulo { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public decimal PrecioTotal { get; set; }
    public string Estado { get; set; }
}
