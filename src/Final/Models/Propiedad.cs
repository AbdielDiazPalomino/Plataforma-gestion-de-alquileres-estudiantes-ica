namespace Final.Models;

public class Propiedad
{
    public int Id { get; set; }

    // Usuario que publica/anuncia
    public int PropietarioId { get; set; }
    public Usuario Propietario { get; set; }

    // Información general
    public string Titulo { get; set; }
    public string Descripcion { get; set; }

    // Ubicación completa
    public string Departamento { get; set; } = "Ica"; // fijo
    public string Provincia { get; set; } = "Ica";    // fijo
    public string Distrito { get; set; }              // ejemplo: "La Tinguiña", "Pachacútec"
    public string Direccion { get; set; }             // Av, Calle, Urb

    // Opcional pero recomendado
    public string Referencia { get; set; }            // "por la UNSLG", "cerca al mall", etc.

    // Coordenadas exactas (para mostrar en mapa)
    public double Latitud { get; set; }
    public double Longitud { get; set; }

    // Datos de la propiedad / filtros
    public decimal PrecioMensual { get; set; }
    public int Habitaciones { get; set; }
    public int Banos { get; set; }
    public bool Amoblado { get; set; }
    public bool ServiciosIncluidos { get; set; }
    public bool InternetIncluido { get; set; }
    public bool AguaIncluida { get; set; }
    public bool LuzIncluida { get; set; }

    // Características adicionales
    public bool AceptaMascotas { get; set; }
    public bool SoloEstudiantes { get; set; } = true; // tu sistema es de estudiantes

    // Fotos (mejor en una tabla aparte, pero puedes empezar simple)
    public List<PropiedadFoto> Fotos { get; set; }

    // Estado
    public bool Aprobada { get; set; } = false;

    // Fechas
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
}

public class PropiedadFoto
{
    public int Id { get; set; }
    public int PropiedadId { get; set; }
    public Propiedad Propiedad { get; set; }
    public string Url { get; set; }
}
