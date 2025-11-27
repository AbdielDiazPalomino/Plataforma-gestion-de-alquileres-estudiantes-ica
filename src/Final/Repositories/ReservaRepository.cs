// Repositories/ReservaRepository.cs
using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
    {
        public ReservaRepository(ApplicationDbContext context) : base(context) { }

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

        // Override para incluir las relaciones
        public override async Task<IEnumerable<Reserva>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Include(r => r.Usuario)
                .ToListAsync();
        }
    }
}