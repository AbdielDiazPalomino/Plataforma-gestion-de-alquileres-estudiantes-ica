using Final.DTOs.Propiedad;
using Final.DTOs.Reserva;
using Final.DTOs.Usuario;
using Final.Models;
using Final.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Final.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IPropiedadService _propiedadService;
    private readonly IUsuarioService _usuarioService;
    private readonly IReservaService _reservaService;
    private readonly IResenaService _resenaService;
    private readonly IDashboardService _dashboardService;

    public AdminController(
        IPropiedadService propiedadService,
        IUsuarioService usuarioService,
        IReservaService reservaService,
        IResenaService resenaService,
        IDashboardService dashboardService)
    {
        _propiedadService = propiedadService;
        _usuarioService = usuarioService;
        _reservaService = reservaService;
        _resenaService = resenaService;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// GET: Dashboard principal con todas las estadísticas
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var stats = await _dashboardService.GetStatsAsync();
            var topPropiedades = await _dashboardService.GetTopPropiedadesAsync();
            
            return Ok(new
            {
                estadisticas = stats,
                topPropiedades,
                fecha = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Propiedades pendientes de aprobación
    /// </summary>
    [HttpGet("propiedades/pendientes")]
    public async Task<IActionResult> GetPropiedadesPendientes([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var propiedades = await _propiedadService.GetPendientesAprobacionAsync();
            var total = propiedades.Count;
            var paginated = propiedades
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                propiedades = paginated,
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// PUT: Aprobar propiedad
    /// </summary>
    [HttpPut("propiedades/{id}/approve")]
    public async Task<IActionResult> ApproveProperty(int id)
    {
        try
        {
            var success = await _propiedadService.AprobarPropiedadAsync(id, true);
            if (!success)
                return BadRequest(new { error = "No se pudo aprobar la propiedad" });

            return Ok(new { 
                message = "Propiedad aprobada exitosamente", 
                propiedadId = id,
                aprobada = true 
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// PUT: Rechazar propiedad
    /// </summary>
    [HttpPut("propiedades/{id}/reject")]
    public async Task<IActionResult> RejectProperty(int id, [FromBody] RejectPropertyDto dto)
    {
        try
        {
            var success = await _propiedadService.AprobarPropiedadAsync(id, false);
            if (!success)
                return BadRequest(new { error = "No se pudo rechazar la propiedad" });

            return Ok(new 
            { 
                message = "Propiedad rechazada exitosamente", 
                propiedadId = id,
                aprobada = false,
                motivo = dto?.MotivoRechazo 
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Todas las propiedades del sistema
    /// </summary>
    [HttpGet("propiedades")]
    public async Task<IActionResult> GetAllPropiedades([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Necesitas un método en el servicio para obtener todas las propiedades (no solo las aprobadas)
            var propiedades = await GetAllPropiedadesForAdminAsync();
            
            var total = propiedades.Count;
            var paginated = propiedades
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                propiedades = paginated,
                estadisticas = new
                {
                    total,
                    aprobadas = propiedades.Count(p => p.Aprobada),
                    pendientes = propiedades.Count(p => !p.Aprobada)
                },
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Todos los usuarios del sistema
    /// </summary>
    [HttpGet("usuarios")]
    public async Task<IActionResult> GetAllUsuarios([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            var total = usuarios.Count;
            var paginated = usuarios
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                usuarios = paginated,
                estadisticas = new
                {
                    total,
                    admins = usuarios.Count(u => u.EsAdmin)
                },
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// PUT: Activar/desactivar usuario
    /// </summary>
    [HttpPut("usuarios/{id}/toggle-active")]
    public async Task<IActionResult> ToggleUsuarioActivo(int id, [FromBody] ToggleActiveDto dto)
    {
        try
        {
            var success = await _usuarioService.ToggleActivoAsync(id, dto.Activo);
            if (!success)
                return BadRequest(new { error = "No se pudo actualizar el estado del usuario" });

            var estado = dto.Activo ? "activado" : "desactivado";
            return Ok(new { 
                message = $"Usuario {estado} exitosamente", 
                usuarioId = id,
                activo = dto.Activo 
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// PUT: Cambiar rol de usuario a admin
    /// </summary>
    [HttpPut("usuarios/{id}/make-admin")]
    public async Task<IActionResult> MakeAdmin(int id, [FromBody] MakeAdminDto dto)
    {
        try
        {
            // Primero necesitas implementar este método en el servicio
            var success = await UpdateUserAdminStatusAsync(id, dto.EsAdmin);
            
            if (!success)
                return BadRequest(new { error = "No se pudo actualizar el rol del usuario" });

            var rol = dto.EsAdmin ? "administrador" : "usuario normal";
            return Ok(new { 
                message = $"Usuario ahora es {rol}", 
                usuarioId = id,
                esAdmin = dto.EsAdmin 
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Reporte de reservas
    /// </summary>
    [HttpGet("reportes/reservas")]
    public async Task<IActionResult> GetReporteReservas(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            // Obtener todas las reservas
            var reservas = await GetAllReservasForAdminAsync();
            
            // Filtrar por fechas si se proporcionan
            if (fechaInicio.HasValue)
                reservas = reservas.Where(r => r.FechaInicio >= fechaInicio.Value).ToList();
            
            if (fechaFin.HasValue)
                reservas = reservas.Where(r => r.FechaFin <= fechaFin.Value).ToList();

            var totalIngresos = reservas
                .Where(r => r.Estado == "confirmada")
                .Sum(r => r.PrecioTotal);

            return Ok(new
            {
                periodo = new { fechaInicio, fechaFin },
                estadisticas = new
                {
                    totalReservas = reservas.Count,
                    reservasConfirmadas = reservas.Count(r => r.Estado == "confirmada"),
                    reservasPendientes = reservas.Count(r => r.Estado == "pendiente"),
                    reservasCanceladas = reservas.Count(r => r.Estado == "cancelada"),
                    ingresosTotales = totalIngresos,
                    promedioReserva = reservas.Any() ? reservas.Average(r => r.PrecioTotal) : 0
                },
                reservasPorMes = reservas
                    .GroupBy(r => new { r.FechaInicio.Year, r.FechaInicio.Month })
                    .Select(g => new 
                    { 
                        mes = $"{g.Key.Year}-{g.Key.Month:D2}", 
                        cantidad = g.Count(),
                        ingresos = g.Where(r => r.Estado == "confirmada").Sum(r => r.PrecioTotal)
                    })
                    .OrderBy(x => x.mes)
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Reporte de propiedades
    /// </summary>
    [HttpGet("reportes/propiedades")]
    public async Task<IActionResult> GetReportePropiedades()
    {
        try
        {
            var propiedades = await GetAllPropiedadesForAdminAsync();
            
            return Ok(new
            {
                estadisticas = new
                {
                    totalPropiedades = propiedades.Count,
                    propiedadesAprobadas = propiedades.Count(p => p.Aprobada),
                    propiedadesPendientes = propiedades.Count(p => !p.Aprobada),
                    propiedadesActivas = propiedades.Count(p => p.Aprobada),
                    promedioPrecio = propiedades.Any() ? propiedades.Average(p => p.PrecioMensual) : 0
                },
                distribucion = new
                {
                    porDistrito = propiedades
                        .GroupBy(p => p.Distrito)
                        .Select(g => new { 
                            distrito = g.Key, 
                            cantidad = g.Count(),
                            porcentaje = (g.Count() / (double)propiedades.Count) * 100 
                        })
                        .OrderByDescending(x => x.cantidad)
                        .ToList(),
                    
                    porPrecio = new
                    {
                        menosDe500 = propiedades.Count(p => p.PrecioMensual < 500),
                        entre500y1000 = propiedades.Count(p => p.PrecioMensual >= 500 && p.PrecioMensual <= 1000),
                        masDe1000 = propiedades.Count(p => p.PrecioMensual > 1000)
                    },
                    
                    porHabitaciones = propiedades
                        .GroupBy(p => p.Habitaciones)
                        .Select(g => new { habitaciones = g.Key, cantidad = g.Count() })
                        .OrderBy(x => x.habitaciones)
                        .ToList()
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Reporte de usuarios
    /// </summary>
    [HttpGet("reportes/usuarios")]
    public async Task<IActionResult> GetReporteUsuarios()
    {
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            var propiedades = await GetAllPropiedadesForAdminAsync();
            var reservas = await GetAllReservasForAdminAsync();

            return Ok(new
            {
                estadisticas = new
                {
                    totalUsuarios = usuarios.Count,
                    usuariosAdmin = usuarios.Count(u => u.EsAdmin),
                    nuevosUltimoMes = usuarios.Count // Aquí necesitarías filtrar por fecha de creación
                },
                actividad = new
                {
                    usuariosConPropiedades = propiedades
                        .GroupBy(p => p.PropietarioId)
                        .Select(g => g.Key)
                        .Distinct()
                        .Count(),
                    
                    usuariosConReservas = reservas
                        .GroupBy(r => r.UsuarioId)
                        .Select(g => g.Key)
                        .Distinct()
                        .Count(),
                    
                    topPropietarios = propiedades
                        .GroupBy(p => p.PropietarioId)
                        .Select(g => new
                        {
                            usuarioId = g.Key,
                            cantidadPropiedades = g.Count(),
                            nombre = usuarios.FirstOrDefault(u => u.Id == g.Key)?.Nombre ?? "Desconocido"
                        })
                        .OrderByDescending(x => x.cantidadPropiedades)
                        .Take(10)
                        .ToList()
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: Eliminar reseña (como admin)
    /// </summary>
    [HttpDelete("resenas/{id}")]
    public async Task<IActionResult> DeleteResena(int id)
    {
        try
        {
            // Como admin, puedes eliminar cualquier reseña sin verificar usuario
            var resena = await _resenaService.GetByIdAsync(id);
            if (resena == null)
                return NotFound(new { error = "Reseña no encontrada" });

            // Necesitarías un método DeleteAdminAsync en el servicio
            // var success = await _resenaService.DeleteAdminAsync(id);
            // return Ok(new { message = "Reseña eliminada por administrador" });
            
            return StatusCode(501, new { error = "Funcionalidad en desarrollo" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Búsqueda avanzada de usuarios
    /// </summary>
    [HttpGet("usuarios/search")]
    public async Task<IActionResult> SearchUsuarios(
        [FromQuery] string email = null,
        [FromQuery] string nombre = null,
        [FromQuery] bool? activo = null,
        [FromQuery] bool? esAdmin = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            
            // Aplicar filtros
            if (!string.IsNullOrEmpty(email))
                usuarios = usuarios.Where(u => 
                    u.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (!string.IsNullOrEmpty(nombre))
                usuarios = usuarios.Where(u => 
                    u.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();

            if (esAdmin.HasValue)
                usuarios = usuarios.Where(u => u.EsAdmin == esAdmin.Value).ToList();

            var total = usuarios.Count;
            var paginated = usuarios
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                usuarios = paginated,
                filtrosAplicados = new { email, nombre, activo, esAdmin },
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    /// <summary>
    /// GET: Búsqueda avanzada de propiedades
    /// </summary>
    [HttpGet("propiedades/search")]
    public async Task<IActionResult> SearchPropiedades(
        [FromQuery] string titulo = null,
        [FromQuery] string distrito = null,
        [FromQuery] bool? aprobada = null,
        [FromQuery] decimal? precioMin = null,
        [FromQuery] decimal? precioMax = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var propiedades = await GetAllPropiedadesForAdminAsync();
            
            // Aplicar filtros
            if (!string.IsNullOrEmpty(titulo))
                propiedades = propiedades.Where(p => 
                    p.Titulo.Contains(titulo, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (!string.IsNullOrEmpty(distrito))
                propiedades = propiedades.Where(p => 
                    p.Distrito.Contains(distrito, StringComparison.OrdinalIgnoreCase)).ToList();

            if (aprobada.HasValue)
                propiedades = propiedades.Where(p => p.Aprobada == aprobada.Value).ToList();

            if (precioMin.HasValue)
                propiedades = propiedades.Where(p => p.PrecioMensual >= precioMin.Value).ToList();

            if (precioMax.HasValue)
                propiedades = propiedades.Where(p => p.PrecioMensual <= precioMax.Value).ToList();

            var total = propiedades.Count;
            var paginated = propiedades
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                propiedades = paginated,
                filtrosAplicados = new { titulo, distrito, aprobada, precioMin, precioMax },
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }

    // Métodos auxiliares privados
    private async Task<List<Propiedad>> GetAllPropiedadesForAdminAsync()
    {
        // Este método necesitaría implementarse en el servicio
        // Por ahora, usamos uno existente y adaptamos
        var propiedadesList = await _propiedadService.GetAllAsync();
        // Necesitarías un método que devuelva las entidades completas, no DTOs
        return new List<Propiedad>(); // Placeholder
    }

    private async Task<List<Reserva>> GetAllReservasForAdminAsync()
    {
        // Este método necesitaría implementarse en el servicio
        // Por ahora, usamos uno existente
        var reservasList = await _reservaService.GetByPropietarioAsync(0); // Obtener todas
        return new List<Reserva>(); // Placeholder
    }

    private async Task<bool> UpdateUserAdminStatusAsync(int userId, bool esAdmin)
    {
        // Este método necesitaría implementarse en el servicio
        // Por ahora, devolvemos un placeholder
        return true;
    }
}

// DTOs para el AdminController
public class RejectPropertyDto
{
    public string? MotivoRechazo { get; set; }
}

public class ToggleActiveDto
{
    public bool Activo { get; set; }
}

public class MakeAdminDto
{
    public bool EsAdmin { get; set; }
}