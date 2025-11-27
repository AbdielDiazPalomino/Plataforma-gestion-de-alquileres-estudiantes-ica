namespace Final.DTOs.Propiedad;

public class PropiedadCreateDto
{
    public string Titulo { get; set; }
    public string Descripcion { get; set; }

    // Ubicación
    public string Distrito { get; set; }
    public string Direccion { get; set; }
    public string Referencia { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }

    // Datos de alquiler
    public decimal PrecioMensual { get; set; }
    public int Habitaciones { get; set; }
    public int Banos { get; set; }

    public bool ServiciosIncluidos { get; set; }
    public bool InternetIncluido { get; set; }
    public bool AguaIncluida { get; set; }
    public bool LuzIncluida { get; set; }
    public bool Amoblado { get; set; }
    public bool AceptaMascotas { get; set; }
    public bool SoloEstudiantes { get; set; }

    // Fotos (base64 ó urls)
    public List<string> Fotos { get; set; }
}
