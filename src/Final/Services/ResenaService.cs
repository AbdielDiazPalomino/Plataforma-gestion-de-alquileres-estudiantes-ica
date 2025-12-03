// Services/ResenaService.cs (corregido)
using Final.DTOs.Resena;
using Final.Models;
using Final.Repositories;

namespace Final.Services
{
    public class ResenaService : IResenaService
    {
        private readonly IResenaRepository _resenaRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ResenaService(IResenaRepository resenaRepository, IReservaRepository reservaRepository, IUsuarioRepository usuarioRepository)
        {
            _resenaRepository = resenaRepository;
            _reservaRepository = reservaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ResenaDto> CreateAsync(ResenaCreateDto dto, int usuarioId)
        {
            // Verificar que el usuario ha tenido una reserva en esta propiedad
            var reservas = await _reservaRepository.GetReservasPorUsuarioAsync(usuarioId);
            var tieneReserva = reservas.Any(r => 
                r.PropiedadId == dto.PropiedadId && 
                r.Estado == "confirmada" && 
                r.FechaFin < DateOnly.FromDateTime(DateTime.Now));

            if (!tieneReserva)
                throw new ArgumentException("Solo puedes dejar reseñas en propiedades donde hayas completado una reserva");

            // Verificar que no existe ya una reseña
            var reseñaExistente = await _resenaRepository.GetByUsuarioAndPropiedadAsync(usuarioId, dto.PropiedadId);
            if (reseñaExistente != null)
                throw new ArgumentException("Ya has dejado una reseña para esta propiedad");

            var resena = new Resena
            {
                UsuarioId = usuarioId,
                PropiedadId = dto.PropiedadId,
                Calificacion = dto.Calificacion,
                Comentario = dto.Comentario,
                Fecha = DateTime.UtcNow
            };

            await _resenaRepository.AddAsync(resena);
            await _resenaRepository.SaveChangesAsync();
            
            return await MapToDto(resena);
        }

        public async Task<ResenaDto> UpdateAsync(int id, ResenaCreateDto dto, int usuarioId)
        {
            var resena = await _resenaRepository.GetByIdAsync(id);
            if (resena == null)
                throw new ArgumentException("Reseña no encontrada");

            if (resena.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permisos para editar esta reseña");

            resena.Calificacion = dto.Calificacion;
            resena.Comentario = dto.Comentario;

            _resenaRepository.Update(resena);
            await _resenaRepository.SaveChangesAsync();
            
            return await MapToDto(resena);
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            var resena = await _resenaRepository.GetByIdAsync(id);
            if (resena == null)
                throw new ArgumentException("Reseña no encontrada");

            if (resena.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permisos para eliminar esta reseña");

            _resenaRepository.Remove(resena);
            return await _resenaRepository.SaveChangesAsync() > 0;
        }

        public async Task<ResenaDto> GetByIdAsync(int id)
        {
            var resena = await _resenaRepository.GetByIdAsync(id);
            if (resena == null)
                throw new ArgumentException("Reseña no encontrada");

            return await MapToDto(resena);
        }

        public async Task<List<ResenaDto>> GetByPropiedadAsync(int propiedadId)
        {
            var resenas = await _resenaRepository.GetResenasPorPropiedadAsync(propiedadId);
            var dtos = new List<ResenaDto>();

            foreach (var resena in resenas)
            {
                dtos.Add(await MapToDto(resena));
            }

            return dtos;
        }

        public async Task<List<ResenaDto>> GetByUsuarioAsync(int usuarioId)
        {
            var resenas = await _resenaRepository.GetByUsuarioAsync(usuarioId);
            var dtos = new List<ResenaDto>();

            foreach (var resena in resenas)
            {
                dtos.Add(await MapToDto(resena));
            }

            return dtos;
        }

        public async Task<double> GetCalificacionPromedioAsync(int propiedadId)
        {
            var resenas = await _resenaRepository.GetResenasPorPropiedadAsync(propiedadId);
            return resenas.Any() ? resenas.Average(r => r.Calificacion) : 0;
        }

        private async Task<ResenaDto> MapToDto(Resena resena)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(resena.UsuarioId);
            
            return new ResenaDto
            {
                Id = resena.Id,
                UsuarioNombre = usuario?.Nombre ?? "Usuario desconocido",
                Calificacion = resena.Calificacion,
                Comentario = resena.Comentario,
                Fecha = resena.Fecha
            };
        }
    }
}