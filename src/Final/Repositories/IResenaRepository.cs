using Final.Models;

namespace Final.Repositories
{
    public interface IResenaRepository : IGenericRepository<Resena>
    {
        Task<IEnumerable<Resena>> GetResenasPorPropiedadAsync(int propiedadId);
    }
}
