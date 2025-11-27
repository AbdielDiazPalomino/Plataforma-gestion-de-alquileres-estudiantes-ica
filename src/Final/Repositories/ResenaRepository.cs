// ResenaRepository.cs
using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class ResenaRepository : GenericRepository<Resena>, IResenaRepository
    {
        public ResenaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Resena>> GetResenasPorPropiedadAsync(int propiedadId)
        {
            return await _dbSet
                .Include(r => r.Usuario)
                .Where(r => r.PropiedadId == propiedadId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Resena>> GetByUsuarioAsync(int usuarioId)
        {
            return await _dbSet
                .Include(r => r.Propiedad)
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Resena> GetByUsuarioAndPropiedadAsync(int usuarioId, int propiedadId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.PropiedadId == propiedadId);
        }

        public async Task<Usuario> GetUsuarioByResenaIdAsync(int resenaId)
        {
            var resena = await _dbSet
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == resenaId);

            return resena?.Usuario;
        }

    }
}