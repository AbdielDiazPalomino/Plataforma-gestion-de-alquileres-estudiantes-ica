namespace Final.Models;

public class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; }
    public string Email { get; set; }

    public string PasswordHash { get; set; }

    // Nuevos campos opcionales
    public string? Telefono { get; set; } 
    public string? Direccion { get; set; } 

    public bool EsAdmin { get; set; } = false;

    // Para posible deshabilitación en futuro
    public bool Activo { get; set; } = true;

    // Navegación
    public List<Propiedad> PropiedadesPublicadas { get; set; }
    public List<Reserva> Reservas { get; set; }
    public List<Resena> Resenas { get; set; }
}
