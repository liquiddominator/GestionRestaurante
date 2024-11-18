using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using restaurante.Models;
using restaurante.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace restaurante.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly RestauranteWebContext _context;

        public AuthController(AuthService authService, RestauranteWebContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (true)
            {
                var user = await _authService.AuthenticateUser(model.Email, model.Password);
                if (user != null)
                {
                    await SignInUser(user);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Email o contraseña inválidos.");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var model = new RegisterViewModel
            {
                FechaCreacion = DateTime.Now,
                RolesDisponibles = _context.Roles.Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.Nombre
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (true)
            {
                try
                {
                    var user = await _authService.RegisterUser(model.Nombre, model.Apellido, model.Email, model.Contrasena, model.FechaCreacion, model.IdRol);
                    await SignInUser(user);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            model.RolesDisponibles = _context.Roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        private async Task SignInUser(Usuario user)
        {
            var userRole = await _context.UsuarioRoles
                .Include(ur => ur.IdRolNavigation)
                .FirstOrDefaultAsync(ur => ur.IdUsuario == user.IdUsuario);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim("FullName", $"{user.Nombre} {user.Apellido}"),
                new Claim(ClaimTypes.Role, userRole.IdRolNavigation.Nombre)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20) // Ajustado a 20 minutos como en tu Program.cs
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }

    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Creación")]
        public DateTime? FechaCreacion { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        public List<SelectListItem> RolesDisponibles { get; set; }
    }

}
