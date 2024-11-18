using Microsoft.EntityFrameworkCore;
using restaurante.Models;
using System.Security.Cryptography;

namespace restaurante.Services
{
    public class AuthService
    {
        private readonly RestauranteWebContext _context;

        public AuthService(RestauranteWebContext context)
        {
            _context = context;
        }

        public async Task<Usuario> RegisterUser(string nombre, string apellido, string email, string password, DateTime? fechaCreacion, int idRol)
        {
            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                throw new Exception("El email ya está registrado");
            }

            var newUser = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                Contrasena = HashPassword(password),
                FechaCreacion = fechaCreacion ?? DateTime.Now,
                UltimoAcceso = null
            };

            _context.Usuarios.Add(newUser);
            await _context.SaveChangesAsync();

            var usuarioRole = new UsuarioRole
            {
                IdUsuario = newUser.IdUsuario,
                IdRol = idRol,
                FechaAsignacion = DateTime.Now
            };
            _context.UsuarioRoles.Add(usuarioRole);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task<Usuario> AuthenticateUser(string email, string password)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !VerifyPassword(password, user.Contrasena))
            {
                return null;
            }

            user.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
