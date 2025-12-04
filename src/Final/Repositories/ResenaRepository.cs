using Final.Data;
using Final.Models;
using Microsoft.EntityFrameworkCore;

namespace Final.Repositories
{
    public class ResenaRepository : IResenaRepository
    {
        private readonly AppDbContext _context;

        public ResenaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Resena> GetByIdAsync(int id)
        {
            return await _context.Resenas
                .Include(r => r.Usuario)
                .Include(r => r.Propiedad)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Resena>> GetAllAsync()
        {
            return await _context.Resenas
                .Include(r => r.Usuario)
                .Include(r => r.Propiedad)
                .ToListAsync();
        }

        public async Task<List<Resena>> GetByPropiedadAsync(int propiedadId)
        {
            return await _context.Resenas
                .Include(r => r.Usuario)
                .Where(r => r.PropiedadId == propiedadId)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }

        public async Task<List<Resena>> GetByUsuarioAsync(int usuarioId)
        {
            return await _context.Resenas
                .Include(r => r.Propiedad)
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }

        public async Task<Resena> AddAsync(Resena resena)
        {
            await _context.Resenas.AddAsync(resena);
            await SaveChangesAsync();
            return resena;
        }

        public void Update(Resena resena)
        {
            _context.Resenas.Update(resena);
        }

        public void Remove(Resena resena)
        {
            _context.Resenas.Remove(resena);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Resenas.AnyAsync(r => r.Id == id);
        }

        public async Task<double> GetPromedioCalificacionAsync(int propiedadId)
        {
            var resenas = await _context.Resenas
                .Where(r => r.PropiedadId == propiedadId)
                .ToListAsync();

            if (!resenas.Any())
                return 0;

            return Math.Round(resenas.Average(r => r.Calificacion), 1);
        }

        public async Task<bool> UsuarioYaResenoAsync(int usuarioId, int propiedadId)
        {
            return await _context.Resenas
                .AnyAsync(r => r.UsuarioId == usuarioId && r.PropiedadId == propiedadId);
        }

        public async Task<bool> UsuarioTieneReservaCompletadaAsync(int usuarioId, int propiedadId)
        {
            // Esta validación debería estar en el servicio de reservas
            // Por ahora, asumimos que si tiene una reserva con estado "completada"
            return await _context.Reservas
                .AnyAsync(r => r.UsuarioId == usuarioId &&
                               r.PropiedadId == propiedadId &&
                               r.Estado.ToLower() == "completada");
        }
    }
}