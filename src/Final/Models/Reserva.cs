namespace Final.Models;

public class Reserva
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }

    public int PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public decimal PrecioTotal { get; set; }
    public string Estado { get; set; } // pendiente, pagado, cancelado
}
