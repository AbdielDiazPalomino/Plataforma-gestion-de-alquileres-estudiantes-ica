using Final.DTOs.Resena;
using Final.Models;

namespace Final.Services
{
    public interface IResenaService
    {
        Task<Resena> CreateAsync(ResenaCreateDto dto, int usuarioId);
        Task<Resena> UpdateAsync(int id, ResenaUpdateDto dto, int usuarioId);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<ResenaDto> GetByIdAsync(int id);
        Task<List<ResenaDto>> GetByPropiedadAsync(int propiedadId);
        Task<List<ResenaDto>> GetByUsuarioAsync(int usuarioId);
        Task<double> GetCalificacionPromedioAsync(int propiedadId);
        Task<bool> UsuarioPuedeResenarAsync(int usuarioId, int propiedadId);
        Task<bool> UsuarioYaResenoAsync(int usuarioId, int propiedadId);
    }
}