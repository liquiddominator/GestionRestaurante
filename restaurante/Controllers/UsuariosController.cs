using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restaurante.Models;

namespace restaurante.Controllers
{
    [Authorize(Roles = "administrador")]
    public class UsuariosController : Controller
    {
        private readonly RestauranteWebContext _context;

        public UsuariosController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var viewModel = new UsuariosViewModel
            {
                Usuarios = await _context.Usuarios
                    .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.IdRolNavigation)
                    .ToListAsync(),
                Roles = await _context.Roles.ToListAsync(),
                UsuarioRoles = await _context.UsuarioRoles
                    .Include(ur => ur.IdRolNavigation)
                    .Include(ur => ur.IdUsuarioNavigation)
                    .ToListAsync()
            };
            return View(viewModel);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["Roles"] = new MultiSelectList(_context.Roles, "IdRol", "Nombre");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuario,Nombre,Apellido,Email,Contrasena,FechaCreacion,UltimoAcceso")] Usuario usuario, List<int> selectedRoles)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                if (selectedRoles != null)
                {
                    foreach (var roleId in selectedRoles)
                    {
                        _context.UsuarioRoles.Add(new UsuarioRole
                        {
                            IdUsuario = usuario.IdUsuario,
                            IdRol = roleId,
                            FechaAsignacion = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["Roles"] = new MultiSelectList(_context.Roles, "IdRol", "Nombre", selectedRoles);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["Roles"] = new MultiSelectList(_context.Roles, "IdRol", "Nombre", usuario.UsuarioRoles.Select(ur => ur.IdRol));
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,Nombre,Apellido,Email,Contrasena,FechaCreacion,UltimoAcceso")] Usuario usuario, List<int> selectedRoles)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    var existingRoles = await _context.UsuarioRoles.Where(ur => ur.IdUsuario == id).ToListAsync();
                    _context.UsuarioRoles.RemoveRange(existingRoles);

                    if (selectedRoles != null)
                    {
                        foreach (var roleId in selectedRoles)
                        {
                            _context.UsuarioRoles.Add(new UsuarioRole
                            {
                                IdUsuario = usuario.IdUsuario,
                                IdRol = roleId,
                                FechaAsignacion = DateTime.Now
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Roles"] = new MultiSelectList(_context.Roles, "IdRol", "Nombre", selectedRoles);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario != null)
            {
                // Primero, elimina los roles asociados al usuario
                _context.UsuarioRoles.RemoveRange(usuario.UsuarioRoles);

                // Luego, elimina el usuario
                _context.Usuarios.Remove(usuario);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }

        // Métodos para manejar Roles

        // GET: Roles
        public async Task<IActionResult> RolesIndex()
        {
            return View(await _context.Roles.ToListAsync());
        }

        // GET: Roles/Create
        public IActionResult CreateRole()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole([Bind("Nombre,Descripcion")] Role role)
        {
            if (ModelState.IsValid)
            {
                _context.Add(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // O RedirectToAction("RolesIndex") si tienes una vista separada para roles
            }
            return View(role);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> EditRole(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(int id, [Bind("IdRol,Nombre,Descripcion")] Role role)
        {
            if (id != role.IdRol)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.IdRol))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> DeleteRole(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.IdRol == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleConfirmed(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }
    }

    public class UsuariosViewModel
    {
        public List<Usuario> Usuarios { get; set; }
        public List<Role> Roles { get; set; }
        public List<UsuarioRole> UsuarioRoles { get; set; }
    }
}