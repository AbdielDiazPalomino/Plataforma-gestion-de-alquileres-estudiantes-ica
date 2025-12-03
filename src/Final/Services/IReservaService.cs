// Services/IReservaService.cs
using Final.DTOs.Reserva;
using Final.Models;

namespace Final.Services
{
    public interface IReservaService
    {
        Task<ReservaListDto> CreateAsync(ReservaCreateDto dto, int usuarioId);
        Task<bool> CancelAsync(int id, int usuarioId);
        Task<ReservaListDto> GetByIdAsync(int id);
        Task<List<ReservaListDto>> GetByUsuarioAsync(int usuarioId);
        Task<List<ReservaListDto>> GetByPropietarioAsync(int propietarioId);
        Task<bool> ConfirmarReservaAsync(int id, int propietarioId);
        Task<bool> VerificarDisponibilidadAsync(int propiedadId, DateOnly fechaInicio, DateOnly fechaFin);
    }
}