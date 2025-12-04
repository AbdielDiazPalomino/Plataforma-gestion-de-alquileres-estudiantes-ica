namespace Final.DTOs.Propiedad;

public class PropiedadListDto
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public decimal PrecioMensual { get; set; }
    public string Distrito { get; set; }
    public string Direccion { get; set; }

    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public int Banos { get; set; }
    public bool AceptaMascotas { get; set; }

    public string PrimeraFoto { get; set; }
    public double CalificacionPromedio { get; set; }
}
