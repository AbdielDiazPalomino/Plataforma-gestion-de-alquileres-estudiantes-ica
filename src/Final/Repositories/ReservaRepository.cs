using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
    {
        public ReservaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Reserva>> GetReservasPorUsuarioAsync(int usuarioId)
        {
            return await _dbSet.Where(r => r.UsuarioId == usuarioId).ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetReservasPorPropiedadAsync(int propiedadId)
        {
            return await _dbSet.Where(r => r.PropiedadId == propiedadId).ToListAsync();
        }
    }
}
