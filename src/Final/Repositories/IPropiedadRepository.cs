// IPropiedadRepository.cs
using Final.Models;

namespace Final.Repositories
{
    public interface IPropiedadRepository : IGenericRepository<Propiedad>
    {
        Task<IEnumerable<Propiedad>> BuscarPorFiltrosAsync(string distrito, decimal? precioMin, decimal? precioMax, int? habitaciones);
        Task<IEnumerable<Propiedad>> GetByPropietarioAsync(int propietarioId);
        Task<IEnumerable<Propiedad>> GetPendientesAprobacionAsync();
        Task<IEnumerable<Resena>> GetResenasByPropiedadIdAsync(int propiedadId);
        Task<IEnumerable<Resena>> GetAllResenasAsync();
    }
}