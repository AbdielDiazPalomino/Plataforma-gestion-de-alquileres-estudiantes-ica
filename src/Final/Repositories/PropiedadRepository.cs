// PropiedadRepository.cs
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

        public async Task<IEnumerable<Propiedad>> GetByPropietarioAsync(int propietarioId)
        {
            return await _dbSet.Where(p => p.PropietarioId == propietarioId).ToListAsync();
        }

        public async Task<IEnumerable<Propiedad>> GetPendientesAprobacionAsync()
        {
            return await _dbSet.Where(p => !p.Aprobada).ToListAsync();
        }

        public async Task<IEnumerable<Resena>> GetResenasByPropiedadIdAsync(int propiedadId)
        {
            return await _context.Set<Resena>()
                .Include(r => r.Usuario)
                .Where(r => r.PropiedadId == propiedadId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resena>> GetAllResenasAsync()
        {
            return await _context.Set<Resena>().ToListAsync();
        }
    }
}