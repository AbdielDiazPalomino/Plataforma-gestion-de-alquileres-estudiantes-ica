using Final.DTOs.Usuario;
using Final.Services;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UsuarioRegisterDto dto)
    {
        try
        {
            await _usuarioService.RegisterAsync(dto);
            // After registering, login to return token
            var loginDto = new UsuarioLoginDto { Email = dto.Email, Password = dto.Password };
            var (usuario, token) = await _usuarioService.LoginAsync(loginDto);
            return Ok(new { usuario, token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UsuarioLoginDto dto)
    {
        try
        {
            var (usuario, token) = await _usuarioService.LoginAsync(dto);
            return Ok(new { usuario, token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
