namespace Final.DTOs.Usuario
{
    public class UsuarioRegisterDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

    }
}