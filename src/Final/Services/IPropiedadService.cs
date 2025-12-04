// Services/IPropiedadService.cs (actualizado)
using Final.DTOs.Propiedad;
using Final.Models;

namespace Final.Services
{
    public interface IPropiedadService
    {
        Task<PropiedadDetailDto> CreateAsync(PropiedadCreateDto dto, int propietarioId);
        Task<PropiedadDetailDto> UpdateAsync(int id, PropiedadUpdateDto dto, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<PropiedadDetailDto> GetByIdAsync(int id);
        Task<List<PropiedadListDto>> GetAllAsync();
        Task<List<PropiedadListDto>> GetByFiltersAsync(decimal? precioMin, decimal? precioMax, int? habitaciones, string distrito, bool? amoblado);
        Task<List<PropiedadListDto>> GetByPropietarioAsync(int propietarioId);
        Task<bool> AprobarPropiedadAsync(int id, bool aprobada);
        Task<List<PropiedadListDto>> GetPendientesAprobacionAsync();
        Task<List<Propiedad>> GetAllPropiedadesAsync();
        Task<Propiedad?> GetPropiedadForUpdateAsync(int id);
        Task UploadPhotosAsync(int id, List<string> photoUrls);
    }
}