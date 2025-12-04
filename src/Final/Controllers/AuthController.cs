using Final.DTOs.Auth;
using Final.DTOs.Usuario;
using Final.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Registro de nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UsuarioRegisterDto dto)
    {
        try
        {
            var usuario = await _usuarioService.RegisterAsync(dto);
            return Ok(new { 
                message = "Registro exitoso", 
                usuarioId = usuario.Id,
                nextStep = "Por favor inicia sesión" 
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Inicio de sesión
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UsuarioLoginDto dto)
    {
        try
        {
            var (usuario, token) = await _usuarioService.LoginAsync(dto);
            return Ok(new { 
                usuario, 
                token,
                expiresIn = 28800 // 8 horas en segundos
            });
        }
        catch (ArgumentException)
        {
            return Unauthorized(new { error = "Credenciales inválidas" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener información del usuario actual
    /// </summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                return Unauthorized();

            var usuario = await _usuarioService.GetByIdAsync(id);
            return Ok(usuario);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Cerrar sesión (lado cliente - token se elimina en frontend)
    /// </summary>
    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "Sesión cerrada exitosamente" });
    }

    /// <summary>
    /// Solicitar recuperación de contraseña
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            var token = await _usuarioService.GenerarTokenRecuperacionAsync(dto.Email);
            // En producción: enviar email con token
            return Ok(new { 
                message = "Se ha enviado un enlace de recuperación a tu email",
                token = token // Solo para desarrollo
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Cambiar contraseña con token de recuperación
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            var success = await _usuarioService.RecuperarPasswordAsync(dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest(new { error = "Token inválido o expirado" });

            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}