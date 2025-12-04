using Microsoft.EntityFrameworkCore;
using Final.Models;

namespace Final.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Mapeo de todas las tablas:
        public DbSet<Propiedad> Propiedades { get; set; }
        public DbSet<PropiedadFoto> PropiedadFotos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Resena> Resenas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.EsAdmin).HasDefaultValue(false);
                entity.Property(u => u.Activo).HasDefaultValue(true);
            });

            // Configuración de Propiedad
            modelBuilder.Entity<Propiedad>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Descripcion).HasMaxLength(2000);
                entity.Property(p => p.Departamento).HasDefaultValue("Ica").HasMaxLength(50);
                entity.Property(p => p.Provincia).HasDefaultValue("Ica").HasMaxLength(50);
                entity.Property(p => p.Distrito).HasMaxLength(100);
                entity.Property(p => p.Direccion).HasMaxLength(300);
                entity.Property(p => p.Referencia).HasMaxLength(500);
                entity.Property(p => p.PrecioMensual).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Habitaciones).HasDefaultValue(1);
                entity.Property(p => p.Banos).HasDefaultValue(1);
                entity.Property(p => p.Amoblado).HasDefaultValue(false);
                entity.Property(p => p.Aprobada).HasDefaultValue(false);
                entity.Property(p => p.FechaPublicacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relaciones
                entity.HasOne(p => p.Propietario)
                    .WithMany(u => u.PropiedadesPublicadas)
                    .HasForeignKey(p => p.PropietarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.Fotos)
                    .WithOne(pf => pf.Propiedad)
                    .HasForeignKey(pf => pf.PropiedadId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de PropiedadFoto
            modelBuilder.Entity<PropiedadFoto>(entity =>
            {
                entity.HasKey(pf => pf.Id);
                entity.Property(pf => pf.Url).IsRequired().HasMaxLength(500);
                
                entity.HasOne(pf => pf.Propiedad)
                    .WithMany(p => p.Fotos)
                    .HasForeignKey(pf => pf.PropiedadId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Reserva
            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Estado).HasDefaultValue("pendiente").HasMaxLength(20);
                entity.Property(r => r.PrecioTotal).HasColumnType("decimal(10,2)");
                
                entity.HasOne(r => r.Usuario)
                    .WithMany(u => u.Reservas)
                    .HasForeignKey(r => r.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Propiedad)
                    .WithMany()
                    .HasForeignKey(r => r.PropiedadId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Resena
            modelBuilder.Entity<Resena>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Calificacion).IsRequired();
                entity.Property(r => r.Comentario).HasMaxLength(1000);
                entity.Property(r => r.Fecha).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(r => r.Usuario)
                    .WithMany(u => u.Resenas)
                    .HasForeignKey(r => r.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Propiedad)
                    .WithMany()
                    .HasForeignKey(r => r.PropiedadId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}