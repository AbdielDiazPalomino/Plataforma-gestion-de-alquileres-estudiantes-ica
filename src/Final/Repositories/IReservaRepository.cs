using Final.Models;

namespace Final.Repositories
{
    public interface IReservaRepository : IGenericRepository<Reserva>
    {
        Task<IEnumerable<Reserva>> GetReservasPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Reserva>> GetReservasPorPropiedadAsync(int propiedadId);
    }
}
