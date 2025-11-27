using Final.DTOs.Usuario;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Final.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim)) return Unauthorized();

        if (!int.TryParse(idClaim, out var userId)) return Unauthorized();

        var usuario = await _usuarioService.GetByIdAsync(userId);
        return Ok(usuario);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UsuarioRegisterDto dto)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim)) return Unauthorized();

        if (!int.TryParse(idClaim, out var userId)) return Unauthorized();

        var ok = await _usuarioService.UpdateAsync(userId, dto);
        if (!ok) return BadRequest(new { error = "No se pudo actualizar el perfil" });
        return NoContent();
    }
}
