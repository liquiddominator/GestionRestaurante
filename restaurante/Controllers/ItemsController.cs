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
    [Authorize(Roles = "administrador,personal,cliente")]
    public class ItemsController : Controller
    {
        private readonly RestauranteWebContext _context;

        public ItemsController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index(int? categoriaId)
        {
            // Obtener todas las categorías para el filtro
            ViewData["Categorias"] = new SelectList(_context.CategoriasMenus, "IdCategoria", "Nombre");


            // Obtener ítems, filtrando por categoría si se selecciona una
            var items = _context.Items.Include(i => i.IdCategoriaNavigation)
                .Where(i => !categoriaId.HasValue || i.IdCategoria == categoriaId);

            return View(await items.ToListAsync());
        }


        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdItem == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        [Authorize(Roles = "administrador,personal")]
        public IActionResult Create()
        {
            ViewData["IdCategoria"] = new SelectList(_context.CategoriasMenus, "IdCategoria", "Nombre");
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdItem,Nombre,Descripcion,Precio,IdCategoria,Disponible,ImagenUrl,TiempoPreparacion")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCategoria"] = new SelectList(_context.CategoriasMenus, "IdCategoria", "Nombre", item.IdCategoria);
            return View(item);
        }

        // GET: Items/Edit/5
        [Authorize(Roles = "administrador,personal")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["IdCategoria"] = new SelectList(_context.CategoriasMenus, "IdCategoria", "Nombre", item.IdCategoria);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdItem,Nombre,Descripcion,Precio,IdCategoria,Disponible,ImagenUrl,TiempoPreparacion")] Item item)
        {
            if (id != item.IdItem)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.IdItem))
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
            ViewData["IdCategoria"] = new SelectList(_context.CategoriasMenus, "IdCategoria", "Nombre", item.IdCategoria);
            return View(item);
        }

        // GET: Items/Delete/5
        [Authorize(Roles = "administrador,personal")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdItem == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.IdItem == id);
        }
    }
}
