using Final.Models;

namespace Final.Repositories
{
    public interface IPropiedadRepository : IGenericRepository<Propiedad>
    {
        Task<IEnumerable<Propiedad>> BuscarPorFiltrosAsync(string distrito, decimal? precioMin, decimal? precioMax, int? habitaciones);
    }
}
