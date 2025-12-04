// Services/IUsuarioService.cs
using Final.DTOs.Usuario;
using Final.Models;

namespace Final.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDto> RegisterAsync(UsuarioRegisterDto dto);
        Task<(UsuarioResponseDto usuario, string token)> LoginAsync(UsuarioLoginDto dto);
        Task<UsuarioResponseDto> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UsuarioRegisterDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> CambiarPasswordAsync(int id, string currentPassword, string newPassword);
        Task<string> GenerarTokenRecuperacionAsync(string email);
        Task<bool> RecuperarPasswordAsync(string token, string newPassword);
        Task<List<UsuarioResponseDto>> GetAllAsync();
        Task<bool> ToggleActivoAsync(int id, bool activo);
        Task<UsuarioResponseDto> UpdateProfileAsync(int id, UsuarioUpdateDto dto);
    }
}