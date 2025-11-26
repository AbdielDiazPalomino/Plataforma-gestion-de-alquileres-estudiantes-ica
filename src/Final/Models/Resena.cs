namespace Final.Models;

public class Resena
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }

    public int PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; }

    public int Calificacion { get; set; } // 1-5
    public string Comentario { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
