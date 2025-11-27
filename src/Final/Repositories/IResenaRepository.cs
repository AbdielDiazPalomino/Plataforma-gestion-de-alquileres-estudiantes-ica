// IResenaRepository.cs
using Final.Models;

namespace Final.Repositories
{
    public interface IResenaRepository : IGenericRepository<Resena>
    {
        Task<IEnumerable<Resena>> GetResenasPorPropiedadAsync(int propiedadId);
        Task<IEnumerable<Resena>> GetByUsuarioAsync(int usuarioId);
        Task<Resena> GetByUsuarioAndPropiedadAsync(int usuarioId, int propiedadId);
        Task<Usuario> GetUsuarioByResenaIdAsync(int resenaId);
    }
}