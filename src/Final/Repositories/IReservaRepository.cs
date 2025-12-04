// Repositories/IReservaRepository.cs
using Final.Models;

namespace Final.Repositories
{
    public interface IReservaRepository : IGenericRepository<Reserva>
    {
        Task<IEnumerable<Reserva>> GetReservasPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Reserva>> GetReservasPorPropiedadAsync(int propiedadId);
        Task<IEnumerable<Reserva>> GetByPropietarioAsync(int propietarioId);
        
        // Métodos específicos para validación de reseñas
        Task<bool> UsuarioTieneReservaCompletadaAsync(int usuarioId, int propiedadId);
        Task<List<Reserva>> GetReservasCompletadasByUsuarioAsync(int usuarioId);
        Task<bool> ExisteReservaEnRangoAsync(int propiedadId, DateTime fechaInicio, DateTime fechaFin);
        
        // Métodos adicionales útiles
        Task<IEnumerable<Reserva>> GetReservasActivasAsync();
        Task<IEnumerable<Reserva>> GetReservasPendientesAsync();
        
        
    }
}