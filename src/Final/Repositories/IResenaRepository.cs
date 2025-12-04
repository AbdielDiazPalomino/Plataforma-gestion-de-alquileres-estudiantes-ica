// Repositories/IResenaRepository.cs
using Final.Models;

namespace Final.Repositories
{
    public interface IResenaRepository
    {
        Task<Resena> GetByIdAsync(int id);
        Task<List<Resena>> GetByPropiedadAsync(int propiedadId);
        Task<List<Resena>> GetByUsuarioAsync(int usuarioId);
        Task<Resena> AddAsync(Resena resena);
        void Update(Resena resena);
        void Remove(Resena resena);
        Task<bool> SaveChangesAsync();
        Task<bool> ExistsAsync(int id);
        Task<double> GetPromedioCalificacionAsync(int propiedadId);
        Task<bool> UsuarioYaResenoAsync(int usuarioId, int propiedadId);
        Task<bool> UsuarioTieneReservaCompletadaAsync(int usuarioId, int propiedadId);
        Task<List<Resena>> GetAllAsync();
        
    }
}