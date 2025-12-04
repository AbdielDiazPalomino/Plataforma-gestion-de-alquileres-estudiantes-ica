// Repositories/IPropiedadRepository.cs
using Final.Models;

namespace Final.Repositories
{
    public interface IPropiedadRepository : IGenericRepository<Propiedad>
    {
        // Métodos específicos de Propiedad
        Task<List<Propiedad>> BuscarPorFiltrosAsync(string distrito, decimal? precioMin, decimal? precioMax, int? habitaciones);
        Task<List<Propiedad>> GetByPropietarioAsync(int propietarioId);
        Task<List<Propiedad>> GetPendientesAprobacionAsync();
        Task<List<Resena>> GetResenasByPropiedadIdAsync(int propiedadId);
        
    }
}