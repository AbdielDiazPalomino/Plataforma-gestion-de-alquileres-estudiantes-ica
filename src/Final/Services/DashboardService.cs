// Services/DashboardService.cs
using Final.Repositories;

namespace Final.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPropiedadRepository _propiedadRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly IResenaRepository _resenaRepository;

        public DashboardService(
            IUsuarioRepository usuarioRepository, 
            IPropiedadRepository propiedadRepository, 
            IReservaRepository reservaRepository,
            IResenaRepository resenaRepository)
        {
            _usuarioRepository = usuarioRepository;
            _propiedadRepository = propiedadRepository;
            _reservaRepository = reservaRepository;
            _resenaRepository = resenaRepository;
        }

        public async Task<DashboardStats> GetStatsAsync()
        {
            // Avoid running multiple EF Core operations in parallel on the same
            // DbContext (causes "A second operation was started on this context instance...").
            // Execute sequentially or use repository methods that run aggregations in the DB.

            var usuarios = await _usuarioRepository.GetAllAsync();
            var propiedades = await _propiedadRepository.GetAllAsync();
            var reservas = await _reservaRepository.GetAllAsync();

            return new DashboardStats
            {
                TotalUsuarios = usuarios.Count(),
                TotalPropiedades = propiedades.Count(),
                TotalReservas = reservas.Count(r => r.Estado == "confirmada"),
                PropiedadesPendientes = propiedades.Count(p => !p.Aprobada),
                IngresosTotales = reservas.Where(r => r.Estado == "confirmada").Sum(r => r.PrecioTotal)
            };
        }

        public async Task<List<PropiedadStats>> GetTopPropiedadesAsync()
        {
            // Execute queries sequentially to avoid DbContext concurrency issues.
            var propiedades = await _propiedadRepository.GetAllAsync();
            var reservas = await _reservaRepository.GetAllAsync();
            var resenas = await _resenaRepository.GetAllAsync();

            return propiedades
                .Where(p => p.Aprobada)
                .Select(p => new PropiedadStats
                {
                    PropiedadId = p.Id,
                    Titulo = p.Titulo,
                    TotalReservas = reservas.Count(r => r.PropiedadId == p.Id && r.Estado == "confirmada"),
                    CalificacionPromedio = resenas
                        .Where(r => r.PropiedadId == p.Id)
                        .Select(r => r.Calificacion)
                        .DefaultIfEmpty()
                        .Average()
                })
                .OrderByDescending(p => p.TotalReservas)
                .Take(10)
                .ToList();
        }
    }
}