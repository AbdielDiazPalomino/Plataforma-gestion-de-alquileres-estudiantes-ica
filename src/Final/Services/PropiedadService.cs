// Services/PropiedadService.cs (corregido)
using Final.DTOs.Propiedad;
using Final.Models;
using Final.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Final.Services
{
    public class PropiedadService : IPropiedadService
    {
        private readonly IPropiedadRepository _propiedadRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public PropiedadService(IPropiedadRepository propiedadRepository, IUsuarioRepository usuarioRepository)
        {
            _propiedadRepository = propiedadRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<PropiedadDetailDto> CreateAsync(PropiedadCreateDto dto, int propietarioId)
        {
            var propietario = await _usuarioRepository.GetByIdAsync(propietarioId);
            if (propietario == null)
                throw new ArgumentException("Usuario no encontrado");

            var propiedad = new Propiedad
            {
                PropietarioId = propietarioId,
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Distrito = dto.Distrito,
                Direccion = dto.Direccion,
                Referencia = dto.Referencia,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                PrecioMensual = dto.PrecioMensual,
                Habitaciones = dto.Habitaciones,
                Banos = dto.Banos,
                ServiciosIncluidos = dto.ServiciosIncluidos,
                InternetIncluido = dto.InternetIncluido,
                AguaIncluida = dto.AguaIncluida,
                LuzIncluida = dto.LuzIncluida,
                Amoblado = dto.Amoblado,
                AceptaMascotas = dto.AceptaMascotas,
                SoloEstudiantes = dto.SoloEstudiantes,
                FechaPublicacion = DateTime.UtcNow,
                Fotos = dto.Fotos?.Select(f => new PropiedadFoto { Url = f }).ToList() ?? new List<PropiedadFoto>()
            };

            await _propiedadRepository.AddAsync(propiedad);
            await _propiedadRepository.SaveChangesAsync();
            
            return await MapToDetailDto(propiedad);
        }

        public async Task<PropiedadDetailDto> UpdateAsync(int id, PropiedadUpdateDto dto, int userId)
        {
            var propiedad = await _propiedadRepository.GetByIdAsync(id);
            if (propiedad == null)
                throw new ArgumentException("Propiedad no encontrada");

            if (propiedad.PropietarioId != userId)
                throw new UnauthorizedAccessException("No tienes permisos para editar esta propiedad");

            propiedad.Titulo = dto.Titulo;
            propiedad.Descripcion = dto.Descripcion;
            propiedad.Distrito = dto.Distrito;
            propiedad.Direccion = dto.Direccion;
            propiedad.Referencia = dto.Referencia;
            propiedad.PrecioMensual = dto.PrecioMensual;
            propiedad.Habitaciones = dto.Habitaciones;
            propiedad.Banos = dto.Banos;
            propiedad.ServiciosIncluidos = dto.ServiciosIncluidos;
            propiedad.InternetIncluido = dto.InternetIncluido;
            propiedad.AguaIncluida = dto.AguaIncluida;
            propiedad.LuzIncluida = dto.LuzIncluida;
            propiedad.Amoblado = dto.Amoblado;
            propiedad.AceptaMascotas = dto.AceptaMascotas;
            propiedad.SoloEstudiantes = dto.SoloEstudiantes;

            _propiedadRepository.Update(propiedad);
            await _propiedadRepository.SaveChangesAsync();
            
            return await MapToDetailDto(propiedad);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var propiedad = await _propiedadRepository.GetByIdAsync(id);
            if (propiedad == null)
                throw new ArgumentException("Propiedad no encontrada");

            if (propiedad.PropietarioId != userId)
                throw new UnauthorizedAccessException("No tienes permisos para eliminar esta propiedad");

            _propiedadRepository.Remove(propiedad);
            return await _propiedadRepository.SaveChangesAsync() > 0;
        }

        public async Task<PropiedadDetailDto> GetByIdAsync(int id)
        {
            var propiedad = await _propiedadRepository.GetByIdAsync(id);
            if (propiedad == null)
                throw new ArgumentException("Propiedad no encontrada");

            return await MapToDetailDto(propiedad);
        }

        public async Task<List<PropiedadListDto>> GetAllAsync()
        {
            var propiedades = await _propiedadRepository.GetAllAsync();
            return propiedades.Where(p => p.Aprobada)
                            .Select(MapToListDto)
                            .ToList();
        }

        public async Task<List<PropiedadListDto>> GetByFiltersAsync(decimal? precioMin, decimal? precioMax, int? habitaciones, string distrito, bool? amoblado)
        {
            var propiedades = await _propiedadRepository.BuscarPorFiltrosAsync(distrito, precioMin, precioMax, habitaciones);
            return propiedades.Where(p => p.Aprobada)
                            .Select(MapToListDto)
                            .ToList();
        }

        public async Task<List<PropiedadListDto>> GetByPropietarioAsync(int propietarioId)
        {
            var propiedades = await _propiedadRepository.GetByPropietarioAsync(propietarioId);
            return propiedades.Select(MapToListDto).ToList();
        }

        public async Task<bool> AprobarPropiedadAsync(int id, bool aprobada)
        {
            var propiedad = await _propiedadRepository.GetByIdAsync(id);
            if (propiedad == null)
                throw new ArgumentException("Propiedad no encontrada");

            propiedad.Aprobada = aprobada;
            _propiedadRepository.Update(propiedad);
            return await _propiedadRepository.SaveChangesAsync() > 0;
        }

        public async Task<List<PropiedadListDto>> GetPendientesAprobacionAsync()
        {
            var propiedades = await _propiedadRepository.GetPendientesAprobacionAsync();
            return propiedades.Select(MapToListDto).ToList();
        }

        private async Task<PropiedadDetailDto> MapToDetailDto(Propiedad propiedad)
        {
            var resenas = await _propiedadRepository.GetResenasByPropiedadIdAsync(propiedad.Id);
            var calificacionPromedio = resenas.Any() ? resenas.Average(r => r.Calificacion) : 0;

            return new PropiedadDetailDto
            {
                Id = propiedad.Id,
                Titulo = propiedad.Titulo,
                Descripcion = propiedad.Descripcion,
                Distrito = propiedad.Distrito,
                Direccion = propiedad.Direccion,
                Referencia = propiedad.Referencia,
                PrecioMensual = propiedad.PrecioMensual,
                Habitaciones = propiedad.Habitaciones,
                Banos = propiedad.Banos,
                ServiciosIncluidos = propiedad.ServiciosIncluidos,
                InternetIncluido = propiedad.InternetIncluido,
                AguaIncluida = propiedad.AguaIncluida,
                LuzIncluida = propiedad.LuzIncluida,
                Amoblado = propiedad.Amoblado,
                AceptaMascotas = propiedad.AceptaMascotas,
                SoloEstudiantes = propiedad.SoloEstudiantes,
                Fotos = propiedad.Fotos?.Select(f => f.Url).ToList() ?? new List<string>(),
                CalificacionPromedio = Math.Round(calificacionPromedio, 1),
                Resenas = resenas.Select(r => new DTOs.Resena.ResenaDto
                {
                    Id = r.Id,
                    UsuarioNombre = r.Usuario?.Nombre ?? "Usuario desconocido",
                    Calificacion = r.Calificacion,
                    Comentario = r.Comentario,
                    Fecha = r.Fecha
                }).ToList()
            };
        }

        private PropiedadListDto MapToListDto(Propiedad propiedad)
        {
            return new PropiedadListDto
            {
                Id = propiedad.Id,
                Titulo = propiedad.Titulo,
                PrecioMensual = propiedad.PrecioMensual,
                Distrito = propiedad.Distrito,
                Direccion = propiedad.Direccion,
                Latitud = propiedad.Latitud,
                Longitud = propiedad.Longitud,
                PrimeraFoto = propiedad.Fotos?.FirstOrDefault()?.Url ?? string.Empty,
                CalificacionPromedio = 0 // Se calcularía en una implementación real
            };
        }
    }
}