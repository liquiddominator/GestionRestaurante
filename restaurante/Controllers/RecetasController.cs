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
    [Authorize(Roles = "administrador,personal")]
    public class RecetasController : Controller
    {
        private readonly RestauranteWebContext _context;

        public RecetasController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: Recetas
        public async Task<IActionResult> Index()
        {
            var restauranteWebContext = _context.Recetas.Include(r => r.IdInventarioNavigation).Include(r => r.IdItemNavigation);
            return View(await restauranteWebContext.ToListAsync());
        }

        // GET: Recetas/Details/5/10
        public async Task<IActionResult> Details(int? idItem, int? idInventario)
        {
            if (idItem == null || idInventario == null)
            {
                return NotFound();
            }

            var receta = await _context.Recetas
                .Include(r => r.IdInventarioNavigation)
                .Include(r => r.IdItemNavigation)
                .FirstOrDefaultAsync(m => m.IdItem == idItem && m.IdInventario == idInventario);
            if (receta == null)
            {
                return NotFound();
            }

            return View(receta);
        }

        // GET: Recetas/Create
        public IActionResult Create()
        {
            ViewData["IdInventario"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre");
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre");
            return View();
        }

        // POST: Recetas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdItem,IdInventario,CantidadRequerida")] Receta receta)
        {
            if (true)
            {
                _context.Add(receta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdInventario"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre", receta.IdInventario);
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", receta.IdItem);
            return View(receta);
        }

        // GET: Recetas/Edit/5/10
        public async Task<IActionResult> Edit(int? idItem, int? idInventario)
        {
            if (idItem == null || idInventario == null)
            {
                return NotFound();
            }

            var receta = await _context.Recetas.FindAsync(idItem, idInventario);
            if (receta == null)
            {
                return NotFound();
            }
            ViewData["IdInventario"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre", receta.IdInventario);
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", receta.IdItem);
            return View(receta);
        }

        // POST: Recetas/Edit/5/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int idItem, int idInventario, [Bind("IdItem,IdInventario,CantidadRequerida")] Receta receta)
        {
            if (idItem != receta.IdItem || idInventario != receta.IdInventario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(receta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecetaExists(receta.IdItem, receta.IdInventario))
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
            ViewData["IdInventario"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre", receta.IdInventario);
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", receta.IdItem);
            return View(receta);
        }

        // GET: Recetas/Delete/5/10
        public async Task<IActionResult> Delete(int? idItem, int? idInventario)
        {
            if (idItem == null || idInventario == null)
            {
                return NotFound();
            }

            var receta = await _context.Recetas
                .Include(r => r.IdInventarioNavigation)
                .Include(r => r.IdItemNavigation)
                .FirstOrDefaultAsync(m => m.IdItem == idItem && m.IdInventario == idInventario);
            if (receta == null)
            {
                return NotFound();
            }

            return View(receta);
        }

        // POST: Recetas/Delete/5/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int idItem, int idInventario)
        {
            var receta = await _context.Recetas.FindAsync(idItem, idInventario);
            if (receta != null)
            {
                _context.Recetas.Remove(receta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecetaExists(int idItem, int idInventario)
        {
            return _context.Recetas.Any(e => e.IdItem == idItem && e.IdInventario == idInventario);
        }
    }
}