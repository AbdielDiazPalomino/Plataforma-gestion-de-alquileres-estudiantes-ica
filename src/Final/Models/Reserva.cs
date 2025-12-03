namespace Final.Models;

public class Reserva
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }

    public int PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    // Se llena automáticamente al crear la reserva
    public decimal PrecioTotal { get; set; }

    // Estados permitidos sin pasarela de pago
    public string Estado { get; set; } = "pendiente"; 
    // otros: "confirmada", "cancelada"
}
