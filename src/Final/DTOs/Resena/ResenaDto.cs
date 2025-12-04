namespace Final.DTOs.Resena;

public class ResenaDto
{
    public int Id { get; set; }
    public int PropiedadId { get; set; }
    public string UsuarioNombre { get; set; }
    public int Calificacion { get; set; }
    public string Comentario { get; set; }
    public DateTime Fecha { get; set; }
}
