// Repositories/PropiedadRepository.cs
using Final.Data;
using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class PropiedadRepository : GenericRepository<Propiedad>, IPropiedadRepository
    {
        public PropiedadRepository(AppDbContext context) : base(context) { }

        public async Task<List<Propiedad>> BuscarPorFiltrosAsync(string distrito, decimal? precioMin, decimal? precioMax, int? habitaciones)
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

            return await query
                .Include(p => p.Fotos)
                .Include(p => p.Propietario)
                .ToListAsync();
        }

        public async Task<List<Propiedad>> GetByPropietarioAsync(int propietarioId)
        {
            return await _dbSet
                .Include(p => p.Fotos)
                .Where(p => p.PropietarioId == propietarioId)
                .ToListAsync();
        }

        public async Task<List<Propiedad>> GetPendientesAprobacionAsync()
        {
            return await _dbSet
                .Include(p => p.Fotos)
                .Include(p => p.Propietario)
                .Where(p => !p.Aprobada)
                .ToListAsync();
        }

        public async Task<List<Resena>> GetResenasByPropiedadIdAsync(int propiedadId)
        {
            return await _context.Resenas
                .Include(r => r.Usuario)
                .Where(r => r.PropiedadId == propiedadId)
                .ToListAsync();
        }

        // Override para incluir relaciones en GetAllAsync
        public override async Task<IEnumerable<Propiedad>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Fotos)
                .Include(p => p.Propietario)
                .ToListAsync();
        }

        // Override para incluir relaciones en GetByIdAsync
        public override async Task<Propiedad?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Fotos)
                .Include(p => p.Propietario)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}