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
    public class UsuarioRolesController : Controller
    {
        private readonly RestauranteWebContext _context;

        public UsuarioRolesController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: UsuarioRoles
        public async Task<IActionResult> Index()
        {
            var restauranteWebContext = _context.UsuarioRoles.Include(u => u.IdRolNavigation).Include(u => u.IdUsuarioNavigation);
            return View(await restauranteWebContext.ToListAsync());
        }

        // GET: UsuarioRoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioRole = await _context.UsuarioRoles
                .Include(u => u.IdRolNavigation)
                .Include(u => u.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuarioRol == id);
            if (usuarioRole == null)
            {
                return NotFound();
            }

            return View(usuarioRole);
        }

        // GET: UsuarioRoles/Create
        public IActionResult Create()
        {
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre");
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Email");
            return View();
        }

        // POST: UsuarioRoles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuarioRol,IdUsuario,IdRol,FechaAsignacion")] UsuarioRole usuarioRole)
        {
            if (true)
            {
                _context.Add(usuarioRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuarioRole.IdRol);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Email", usuarioRole.IdUsuario);
            return View(usuarioRole);
        }

        // GET: UsuarioRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioRole = await _context.UsuarioRoles.FindAsync(id);
            if (usuarioRole == null)
            {
                return NotFound();
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuarioRole.IdRol);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Email", usuarioRole.IdUsuario);
            return View(usuarioRole);
        }

        // POST: UsuarioRoles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuarioRol,IdUsuario,IdRol,FechaAsignacion")] UsuarioRole usuarioRole)
        {
            if (id != usuarioRole.IdUsuarioRol)
            {
                return NotFound();
            }

            if (true)
            {
                try
                {
                    _context.Update(usuarioRole);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioRoleExists(usuarioRole.IdUsuarioRol))
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
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuarioRole.IdRol);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Email", usuarioRole.IdUsuario);
            return View(usuarioRole);
        }

        // GET: UsuarioRoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioRole = await _context.UsuarioRoles
                .Include(u => u.IdRolNavigation)
                .Include(u => u.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuarioRol == id);
            if (usuarioRole == null)
            {
                return NotFound();
            }

            return View(usuarioRole);
        }

        // POST: UsuarioRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioRole = await _context.UsuarioRoles.FindAsync(id);
            if (usuarioRole != null)
            {
                _context.UsuarioRoles.Remove(usuarioRole);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioRoleExists(int id)
        {
            return _context.UsuarioRoles.Any(e => e.IdUsuarioRol == id);
        }
    }
}
