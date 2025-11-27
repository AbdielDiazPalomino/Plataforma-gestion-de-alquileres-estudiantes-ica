// Services/IResenaService.cs
using Final.DTOs.Resena;
using Final.Models;

namespace Final.Services
{
    public interface IResenaService
    {
        Task<ResenaDto> CreateAsync(ResenaCreateDto dto, int usuarioId);
        Task<ResenaDto> UpdateAsync(int id, ResenaCreateDto dto, int usuarioId);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<ResenaDto> GetByIdAsync(int id);
        Task<List<ResenaDto>> GetByPropiedadAsync(int propiedadId);
        Task<List<ResenaDto>> GetByUsuarioAsync(int usuarioId);
        Task<double> GetCalificacionPromedioAsync(int propiedadId);
    }
}