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
            var usuariosTask = _usuarioRepository.GetAllAsync();
            var propiedadesTask = _propiedadRepository.GetAllAsync();
            var reservasTask = _reservaRepository.GetAllAsync();

            await Task.WhenAll(usuariosTask, propiedadesTask, reservasTask);

            var usuarios = await usuariosTask;
            var propiedades = await propiedadesTask;
            var reservas = await reservasTask;

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
            var propiedadesTask = _propiedadRepository.GetAllAsync();
            var reservasTask = _reservaRepository.GetAllAsync();
            var resenasTask = _resenaRepository.GetAllAsync();

            await Task.WhenAll(propiedadesTask, reservasTask, resenasTask);

            var propiedades = await propiedadesTask;
            var reservas = await reservasTask;
            var resenas = await resenasTask;

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