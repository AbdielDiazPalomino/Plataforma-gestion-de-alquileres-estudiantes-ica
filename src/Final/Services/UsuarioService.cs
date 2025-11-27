// Services/UsuarioService.cs (corregido)
using Final.DTOs.Usuario;
using Final.Models;
using Final.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Final.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public UsuarioService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        public async Task<UsuarioResponseDto> RegisterAsync(UsuarioRegisterDto dto)
        {
            var existingUser = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ArgumentException("El email ya está registrado");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Activo = true,
                EsAdmin = false
            };

            usuario.PasswordHash = HashPassword(dto.Password);

            await _usuarioRepository.AddAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            return MapToResponseDto(usuario);
        }

        public async Task<(UsuarioResponseDto usuario, string token)> LoginAsync(UsuarioLoginDto dto)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuario == null || !usuario.Activo)
                throw new ArgumentException("Credenciales inválidas");

            if (!VerifyPassword(dto.Password, usuario.PasswordHash))
                throw new ArgumentException("Credenciales inválidas");

            var token = GenerateJwtToken(usuario);
            return (MapToResponseDto(usuario), token);
        }

        public async Task<UsuarioResponseDto> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            return MapToResponseDto(usuario);
        }

        public async Task<bool> UpdateAsync(int id, UsuarioRegisterDto dto)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;

            _usuarioRepository.Update(usuario);
            return await _usuarioRepository.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            _usuarioRepository.Remove(usuario);
            return await _usuarioRepository.SaveChangesAsync() > 0;
        }

        public async Task<bool> CambiarPasswordAsync(int id, string currentPassword, string newPassword)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            if (!VerifyPassword(currentPassword, usuario.PasswordHash))
                throw new ArgumentException("Contraseña actual incorrecta");

            usuario.PasswordHash = HashPassword(newPassword);
            _usuarioRepository.Update(usuario);
            return await _usuarioRepository.SaveChangesAsync() > 0;
        }

        public async Task<string> GenerarTokenRecuperacionAsync(string email)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            var token = Guid.NewGuid().ToString();
            // Guardar token en base de datos con expiración
            return token;
        }

        public async Task<bool> RecuperarPasswordAsync(string token, string newPassword)
        {
            // Validar token y expiración
            // Buscar usuario por token
            // Actualizar contraseña
            return true;
        }

        public async Task<List<UsuarioResponseDto>> GetAllAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            return usuarios.Select(MapToResponseDto).ToList();
        }

        public async Task<bool> ToggleActivoAsync(int id, bool activo)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            usuario.Activo = activo;
            _usuarioRepository.Update(usuario);
            return await _usuarioRepository.SaveChangesAsync() > 0;
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key no está configurada");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.EsAdmin ? "Admin" : "Usuario")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var storedSubHash = parts[1];

            var computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return storedSubHash == computedHash;
        }

        private UsuarioResponseDto MapToResponseDto(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                EsAdmin = usuario.EsAdmin
            };
        }
    }
}