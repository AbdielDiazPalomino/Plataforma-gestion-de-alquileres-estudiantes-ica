using Final.DTOs.Resena;
using Final.Models;
using Final.Repositories;

namespace Final.Services
{
    public class ResenaService : IResenaService
    {
        private readonly IResenaRepository _resenaRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IPropiedadRepository _propiedadRepository;

        public ResenaService(
            IResenaRepository resenaRepository,
            IReservaRepository reservaRepository,
            IPropiedadRepository propiedadRepository)
        {
            _resenaRepository = resenaRepository;
            _reservaRepository = reservaRepository;
            _propiedadRepository = propiedadRepository;
        }

        public async Task<Resena> CreateAsync(ResenaCreateDto dto, int usuarioId)
        {
            // Validar que la propiedad existe
            var propiedad = await _propiedadRepository.GetByIdAsync(dto.PropiedadId);
            if (propiedad == null)
                throw new ArgumentException("Propiedad no encontrada");

            // Validar que el usuario tenga una reserva completada
            var tieneReservaCompletada = await _reservaRepository.UsuarioTieneReservaCompletadaAsync(usuarioId, dto.PropiedadId);
            if (!tieneReservaCompletada)
                throw new ArgumentException("Debes tener una reserva completada para dejar una reseña");

            // Validar que el usuario no haya dejado ya una reseña
            var yaReseno = await _resenaRepository.UsuarioYaResenoAsync(usuarioId, dto.PropiedadId);
            if (yaReseno)
                throw new ArgumentException("Ya has dejado una reseña para esta propiedad");

            // Validar calificación
            if (dto.Calificacion < 1 || dto.Calificacion > 5)
                throw new ArgumentException("La calificación debe estar entre 1 y 5");

            var resena = new Resena
            {
                UsuarioId = usuarioId,
                PropiedadId = dto.PropiedadId,
                Calificacion = dto.Calificacion,
                Comentario = dto.Comentario,
                Fecha = DateTime.UtcNow
            };

            return await _resenaRepository.AddAsync(resena);
        }

        public async Task<Resena> UpdateAsync(int id, ResenaUpdateDto dto, int usuarioId)
        {
            var resena = await _resenaRepository.GetByIdAsync(id);
            if (resena == null)
                throw new ArgumentException("Reseña no encontrada");

            if (resena.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permisos para editar esta reseña");

            // Validar calificación
            if (dto.Calificacion < 1 || dto.Calificacion > 5)
                throw new ArgumentException("La calificación debe estar entre 1 y 5");

            resena.Calificacion = dto.Calificacion;
            resena.Comentario = dto.Comentario;

            _resenaRepository.Update(resena);
            await _resenaRepository.SaveChangesAsync();

            return resena;
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            var resena = await _resenaRepository.GetByIdAsync(id);
            if (resena == null)
                throw new ArgumentException("Reseña no encontrada");

            if (resena.UsuarioId != usuarioId)
                throw new UnauthorizedAccessException("No tienes permisos para eliminar esta reseña");

            _resenaRepository.Remove(resena);
            return await _resenaRepository.SaveChangesAsync();
        }

        public async Task<ResenaDto> GetByIdAsync(int id)
        {
            var resena = await _resenaRepository.GetByIdAsync(id);
            if (resena == null)
                throw new ArgumentException("Reseña no encontrada");

            return MapToDto(resena);
        }

        public async Task<List<ResenaDto>> GetByPropiedadAsync(int propiedadId)
        {
            var resenas = await _resenaRepository.GetByPropiedadAsync(propiedadId);
            return resenas.Select(MapToDto).ToList();
        }

        public async Task<List<ResenaDto>> GetByUsuarioAsync(int usuarioId)
        {
            var resenas = await _resenaRepository.GetByUsuarioAsync(usuarioId);
            return resenas.Select(MapToDto).ToList();
        }

        public async Task<double> GetCalificacionPromedioAsync(int propiedadId)
        {
            return await _resenaRepository.GetPromedioCalificacionAsync(propiedadId);
        }

        public async Task<bool> UsuarioPuedeResenarAsync(int usuarioId, int propiedadId)
        {
            // Verifica si el usuario puede dejar reseña:
            // 1. Tiene una reserva completada
            // 2. No ha dejado ya una reseña
            var tieneReserva = await _reservaRepository.UsuarioTieneReservaCompletadaAsync(usuarioId, propiedadId);
            var yaReseno = await _resenaRepository.UsuarioYaResenoAsync(usuarioId, propiedadId);
            
            return tieneReserva && !yaReseno;
        }

        public async Task<bool> UsuarioYaResenoAsync(int usuarioId, int propiedadId)
        {
            return await _resenaRepository.UsuarioYaResenoAsync(usuarioId, propiedadId);
        }

        private ResenaDto MapToDto(Resena resena)
        {
            return new ResenaDto
            {
                Id = resena.Id,
                UsuarioNombre = resena.Usuario?.Nombre ?? "Usuario",
                Calificacion = resena.Calificacion,
                Comentario = resena.Comentario,
                Fecha = resena.Fecha
            };
        }
    }
}