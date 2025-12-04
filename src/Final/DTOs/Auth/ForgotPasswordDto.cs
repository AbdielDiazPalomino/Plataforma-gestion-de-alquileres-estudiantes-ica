using System.ComponentModel.DataAnnotations;

namespace Final.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;
}