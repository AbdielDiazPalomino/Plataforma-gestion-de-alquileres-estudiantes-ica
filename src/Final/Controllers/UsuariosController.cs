using Final.DTOs.Usuario;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtener todos los usuarios (solo admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            return Ok(usuarios);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener usuario por ID (solo admin o el propio usuario)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && currentUserId != id)
                return Forbid();

            var usuario = await _usuarioService.GetByIdAsync(id);
            return Ok(usuario);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualizar perfil de usuario
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UsuarioUpdateDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var updated = await _usuarioService.UpdateProfileAsync(currentUserId, dto);
            return Ok(updated);
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
    /// Eliminar usuario (solo admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _usuarioService.DeleteAsync(id);
            if (!success)
                return BadRequest(new { error = "No se pudo eliminar el usuario" });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Cambiar contraseña del usuario actual
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _usuarioService.CambiarPasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            
            if (!success)
                return BadRequest(new { error = "No se pudo cambiar la contraseña" });

            return Ok(new { message = "Contraseña cambiada exitosamente" });
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
    /// Desactivar/activar usuario (solo admin)
    /// </summary>
    [HttpPut("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(int id, [FromBody] bool activo)
    {
        try
        {
            var success = await _usuarioService.ToggleActivoAsync(id, activo);
            if (!success)
                return BadRequest(new { error = "No se pudo actualizar el estado del usuario" });

            return Ok(new { message = $"Usuario {(activo ? "activado" : "desactivado")} exitosamente" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException();
        return userId;
    }
}