// Services/ReservaService.cs (corregido)
using Final.DTOs.Reserva;
using Final.Models;
using Final.Repositories;

namespace Final.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IPropiedadRepository _propiedadRepository;

        public ReservaService(IReservaRepository reservaRepository, IPropiedadRepository propiedadRepository)
        {
            _reservaRepository = reservaRepository;
            _propiedadRepository = propiedadRepository;
        }

        public async Task<ReservaListDto> CreateAsync(ReservaCreateDto dto, int usuarioId)
        {
            // Verificar disponibilidad
            var disponible = await VerificarDisponibilidadAsync(dto.PropiedadId, dto.FechaInicio, dto.FechaFin);
            if (!disponible)
                throw new ArgumentException("La propiedad no está disponible para las fechas seleccionadas");

            var propiedad = await _propiedadRepository.GetByIdAsync(dto.PropiedadId);
            if (propiedad == null || !propiedad.Aprobada)
                throw new ArgumentException("Propiedad no disponible");

            // Calcular precio total
            var dias = (dto.FechaFin - dto.FechaInicio).Days;
            var precioTotal = (propiedad.PrecioMensual / 30) * dias;

            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                PropiedadId = dto.PropiedadId,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                PrecioTotal = precioTotal,
                Estado = "pendiente"
            };

            // CORRECCIÓN: Usa AddAsync (ya incluye SaveChanges)
            await _reservaRepository.AddAsync(reserva);
            
            return MapToListDto(reserva);
        }

        public async Task<bool> CancelAsync(int id, int usuarioId)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            if (reserva == null)
                throw new ArgumentException("Reserva no encontrada");

            // Cargar la propiedad para verificar el propietario
            var propiedad = await _propiedadRepository.GetByIdAsync(reserva.PropiedadId);
            if (propiedad == null)
                throw new ArgumentException("Propiedad no encontrada");

            if (reserva.UsuarioId != usuarioId && propiedad.PropietarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permisos para cancelar esta reserva");

            reserva.Estado = "cancelada";
            
            // CORRECCIÓN: Usa UpdateAsync (ya incluye SaveChanges)
            await _reservaRepository.UpdateAsync(reserva);
            
            return true;
        }

        public async Task<ReservaListDto> GetByIdAsync(int id)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            if (reserva == null)
                throw new ArgumentException("Reserva no encontrada");

            // Cargar la propiedad para el título
            var propiedad = await _propiedadRepository.GetByIdAsync(reserva.PropiedadId);
            return MapToListDto(reserva, propiedad);
        }

        public async Task<List<ReservaListDto>> GetByUsuarioAsync(int usuarioId)
        {
            var reservas = await _reservaRepository.GetReservasPorUsuarioAsync(usuarioId);
            var dtos = new List<ReservaListDto>();

            foreach (var reserva in reservas)
            {
                var propiedad = await _propiedadRepository.GetByIdAsync(reserva.PropiedadId);
                dtos.Add(MapToListDto(reserva, propiedad));
            }

            return dtos;
        }

        public async Task<List<ReservaListDto>> GetByPropietarioAsync(int propietarioId)
        {
            var reservas = await _reservaRepository.GetByPropietarioAsync(propietarioId);
            return reservas.Select(r => MapToListDto(r, r.Propiedad)).ToList();
        }

        public async Task<bool> ConfirmarReservaAsync(int id, int propietarioId)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            if (reserva == null)
                throw new ArgumentException("Reserva no encontrada");

            var propiedad = await _propiedadRepository.GetByIdAsync(reserva.PropiedadId);
            if (propiedad == null || propiedad.PropietarioId != propietarioId)
                throw new UnauthorizedAccessException("No tienes permisos para confirmar esta reserva");

            reserva.Estado = "confirmada";
            
            // CORRECCIÓN: Usa UpdateAsync (ya incluye SaveChanges)
            await _reservaRepository.UpdateAsync(reserva);
            
            return true;
        }

        public async Task<bool> VerificarDisponibilidadAsync(int propiedadId, DateTime fechaInicio, DateTime fechaFin)
        {
            var reservasExistentes = await _reservaRepository.GetReservasPorPropiedadAsync(propiedadId);
            
            return !reservasExistentes.Any(r => 
                r.Estado != "cancelada" &&
                ((fechaInicio >= r.FechaInicio && fechaInicio < r.FechaFin) ||
                 (fechaFin > r.FechaInicio && fechaFin <= r.FechaFin) ||
                 (fechaInicio <= r.FechaInicio && fechaFin >= r.FechaFin)));
        }

        private ReservaListDto MapToListDto(Reserva reserva, Propiedad? propiedad = null)
        {
            return new ReservaListDto
            {
                Id = reserva.Id,
                PropiedadTitulo = propiedad?.Titulo ?? "Propiedad no disponible",
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin,
                PrecioTotal = reserva.PrecioTotal,
                Estado = reserva.Estado
            };
        }
    }
}