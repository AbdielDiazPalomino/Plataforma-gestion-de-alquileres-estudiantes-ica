namespace Final.Models;

public class Propiedad
{
    public int Id { get; set; }
    public int PropietarioId { get; set; }
    public Usuario Propietario { get; set; }

    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string Direccion { get; set; }
    public decimal PrecioMensual { get; set; }
    public int Habitaciones { get; set; }
    public bool ServiciosIncluidos { get; set; }
    public string ImagenUrl { get; set; }

    public bool Aprobada { get; set; } = false; // el admin aprueba
}
