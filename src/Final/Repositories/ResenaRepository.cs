using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class ResenaRepository : GenericRepository<Resena>, IResenaRepository
    {
        public ResenaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Resena>> GetResenasPorPropiedadAsync(int propiedadId)
        {
            return await _dbSet.Where(r => r.PropiedadId == propiedadId).ToListAsync();
        }
    }
}
