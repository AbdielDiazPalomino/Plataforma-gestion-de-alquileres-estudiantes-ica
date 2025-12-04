// Repositories/ReservaRepository.cs
using Final.Data;
using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
    {
        public ReservaRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Reserva>> GetReservasPorUsuarioAsync(int usuarioId)
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasPorPropiedadAsync(int propiedadId)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Where(r => r.PropiedadId == propiedadId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetByPropietarioAsync(int propietarioId)
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .Where(r => r.Propiedad.PropietarioId == propietarioId)
                .ToListAsync();
        }

        public async Task<bool> UsuarioTieneReservaCompletadaAsync(int usuarioId, int propiedadId)
        {
            return await _dbSet
                .AnyAsync(r => r.UsuarioId == usuarioId && 
                               r.PropiedadId == propiedadId && 
                               (r.Estado.ToLower() == "confirmada" || r.Estado.ToLower() == "completada"));
        }

        public async Task<List<Reserva>> GetReservasCompletadasByUsuarioAsync(int usuarioId)
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Where(r => r.UsuarioId == usuarioId && 
                           (r.Estado.ToLower() == "confirmada" || r.Estado.ToLower() == "completada"))
                .OrderByDescending(r => r.FechaFin)
                .ToListAsync();
        }

        public async Task<bool> ExisteReservaEnRangoAsync(int propiedadId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await _dbSet
                .AnyAsync(r => r.PropiedadId == propiedadId &&
                               r.Estado != "cancelada" &&
                               ((r.FechaInicio <= fechaFin && r.FechaFin >= fechaInicio)));
        }

        // Override para incluir las relaciones
        public override async Task<IEnumerable<Reserva>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .ToListAsync();
        }

        // Override para obtener por ID con relaciones
        public override async Task<Reserva?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // Método para reservas activas (no canceladas)
        public async Task<IEnumerable<Reserva>> GetReservasActivasAsync()
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .Where(r => r.Estado != "cancelada")
                .ToListAsync();
        }

        // Método para reservas pendientes
        public async Task<IEnumerable<Reserva>> GetReservasPendientesAsync()
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .Where(r => r.Estado == "pendiente")
                .ToListAsync();
        }

        // Método para contar reservas por estado
        public async Task<Dictionary<string, int>> GetEstadisticasReservasAsync()
        {
            var reservas = await _dbSet.ToListAsync();
            return reservas
                .GroupBy(r => r.Estado)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}