// Services/ReservaService.cs (corregido)
using Final.DTOs.Reserva;
using Final.Models;
using Final.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Final.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IPropiedadRepository _propiedadRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ApplicationDbContext _dbContext;

        public ReservaService(IReservaRepository reservaRepository, IPropiedadRepository propiedadRepository, IUsuarioRepository usuarioRepository, ApplicationDbContext dbContext)
        {
            _reservaRepository = reservaRepository;
            _propiedadRepository = propiedadRepository;
            _usuarioRepository = usuarioRepository;
            _dbContext = dbContext;
        }

        public async Task<ReservaListDto> CreateAsync(ReservaCreateDto dto, int usuarioId)
        {
            // Use a transaction at Serializable isolation to avoid race conditions
            // between concurrent reservation creations. We re-check availability
            // inside the transaction and commit only after insertion.
            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    // Re-check availability inside the transaction
                    var disponible = await VerificarDisponibilidadAsync(dto.PropiedadId, dto.FechaInicio, dto.FechaFin);
                    if (!disponible)
                        throw new ArgumentException("La propiedad no está disponible para las fechas seleccionadas");

                    var propiedad = await _propiedadRepository.GetByIdAsync(dto.PropiedadId);
                    if (propiedad == null || !propiedad.Aprobada)
                        throw new ArgumentException("Propiedad no disponible");

                    // Calcular precio total (días entre fechas)
                    var dias = (dto.FechaFin.ToDateTime(TimeOnly.MinValue) - dto.FechaInicio.ToDateTime(TimeOnly.MinValue)).Days;
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

                    await _reservaRepository.AddAsync(reserva);
                    await _reservaRepository.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return await MapToListDto(reserva);
                }
                catch (Exception ex)
                {
                    try { await transaction.RollbackAsync(); } catch { }

                    if (IsTransientConcurrencyException(ex) && attempt < maxRetries - 1)
                    {
                        await Task.Delay(100 * (attempt + 1));
                        continue; // retry
                    }

                    throw;
                }
            }

            throw new InvalidOperationException("No se pudo crear la reserva debido a un conflicto de concurrencia");
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
            _reservaRepository.Update(reserva);
            return await _reservaRepository.SaveChangesAsync() > 0;
        }

        public async Task<ReservaListDto> GetByIdAsync(int id)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            if (reserva == null)
                throw new ArgumentException("Reserva no encontrada");

            // Cargar la propiedad para el título
            var propiedad = await _propiedadRepository.GetByIdAsync(reserva.PropiedadId);
            return await MapToListDto(reserva, propiedad);
        }

        public async Task<List<ReservaListDto>> GetByUsuarioAsync(int usuarioId)
        {
            var reservas = await _reservaRepository.GetReservasPorUsuarioAsync(usuarioId);
            var dtos = new List<ReservaListDto>();

            foreach (var reserva in reservas)
            {
                var propiedad = await _propiedadRepository.GetByIdAsync(reserva.PropiedadId);
                dtos.Add(await MapToListDto(reserva, propiedad));
            }

            return dtos;
        }

        public async Task<List<ReservaListDto>> GetByPropietarioAsync(int propietarioId)
        {
            var reservas = await _reservaRepository.GetByPropietarioAsync(propietarioId);
            var list = new List<ReservaListDto>();
            foreach (var r in reservas)
            {
                list.Add(await MapToListDto(r, r.Propiedad));
            }
            return list;
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
            _reservaRepository.Update(reserva);
            return await _reservaRepository.SaveChangesAsync() > 0;
        }

        public async Task<bool> VerificarDisponibilidadAsync(int propiedadId, DateOnly fechaInicio, DateOnly fechaFin)
        {
            var reservasExistentes = await _reservaRepository.GetReservasPorPropiedadAsync(propiedadId);
            
            return !reservasExistentes.Any(r => 
                r.Estado != "cancelada" &&
                ((fechaInicio >= r.FechaInicio && fechaInicio < r.FechaFin) ||
                 (fechaFin > r.FechaInicio && fechaFin <= r.FechaFin) ||
                 (fechaInicio <= r.FechaInicio && fechaFin >= r.FechaFin)));
        }

        private async Task<ReservaListDto> MapToListDto(Reserva reserva, Propiedad? propiedad = null)
        {
            // Asegurar que tenemos información del usuario
            var usuario = reserva.Usuario ?? await _usuarioRepository.GetByIdAsync(reserva.UsuarioId);

            return new ReservaListDto
            {
                Id = reserva.Id,
                PropiedadTitulo = propiedad?.Titulo ?? "Propiedad no disponible",
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin,
                PrecioTotal = reserva.PrecioTotal,
                Estado = reserva.Estado,
                UsuarioNombre = usuario?.Nombre ?? "Usuario desconocido",
                UsuarioEmail = usuario?.Email ?? string.Empty
            };
        }

        private bool IsTransientConcurrencyException(Exception ex)
        {
            if (ex == null) return false;
            if (ex is DbUpdateConcurrencyException) return true;
            if (ex is DbUpdateException) return true;
            return ex.InnerException != null && IsTransientConcurrencyException(ex.InnerException);
        }
    }
}