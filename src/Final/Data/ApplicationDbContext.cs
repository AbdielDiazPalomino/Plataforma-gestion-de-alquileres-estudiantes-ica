using Microsoft.EntityFrameworkCore;

using Final.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Propiedad> Propiedades { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Resena> Resenas { get; set; }
}
