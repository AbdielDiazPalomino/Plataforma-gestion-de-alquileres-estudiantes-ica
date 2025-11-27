using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class PropiedadRepository : GenericRepository<Propiedad>, IPropiedadRepository
    {
        public PropiedadRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Propiedad>> BuscarPorFiltrosAsync(string distrito, decimal? precioMin, decimal? precioMax, int? habitaciones)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(distrito))
                query = query.Where(p => p.Distrito.Contains(distrito));

            if (precioMin.HasValue)
                query = query.Where(p => p.PrecioMensual >= precioMin.Value);

            if (precioMax.HasValue)
                query = query.Where(p => p.PrecioMensual <= precioMax.Value);

            if (habitaciones.HasValue)
                query = query.Where(p => p.Habitaciones == habitaciones.Value);

            return await query.ToListAsync();
        }
    }
}
