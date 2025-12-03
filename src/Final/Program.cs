using Final.Services;

using Final.Repositories;


using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "StudentHome API",
        Version = "v1"
    });

    // 🔐 CONFIGURACIÓN PARA ACTIVAR EL BOTÓN AUTHORIZE
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT con el formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// En Program.cs
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPropiedadService, PropiedadService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IResenaService, ResenaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Authentication (JWT)
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

    builder.Services.AddAuthorization();
}

// Register repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPropiedadRepository, PropiedadRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IResenaRepository, ResenaRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed admin user in Development for quick testing
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
    var usuarioService = scope.ServiceProvider.GetRequiredService<IUsuarioService>();

    // Create default admin if not exists
    var adminEmail = "admin@gmail.com";
    var existing = await usuarioRepo.GetByEmailAsync(adminEmail);
    if (existing == null)
    {
        var registerDto = new Final.DTOs.Usuario.UsuarioRegisterDto
        {
            Nombre = "Administrador",
            Email = adminEmail,
            Password = "admin123"
        };

        var created = await usuarioService.RegisterAsync(registerDto);

        // mark as admin
        var admin = await usuarioRepo.GetByEmailAsync(adminEmail);
        if (admin != null)
        {
            admin.EsAdmin = true;
            usuarioRepo.Update(admin);
            await usuarioRepo.SaveChangesAsync();
        }
    }
}

app.Run();
