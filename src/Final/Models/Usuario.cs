namespace Final.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Rol { get; set; } // estudiante, propietario, admin
    public bool Estado { get; set; } = true; // activo/deshabilitado
}
