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
    public class CategoriasMenuController : Controller
    {
        private readonly RestauranteWebContext _context;

        public CategoriasMenuController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: CategoriasMenu
        public async Task<IActionResult> Index()
        {
            return View(await _context.CategoriasMenus.ToListAsync());
        }

        // GET: CategoriasMenu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriasMenu = await _context.CategoriasMenus
                .FirstOrDefaultAsync(m => m.IdCategoria == id);
            if (categoriasMenu == null)
            {
                return NotFound();
            }

            return View(categoriasMenu);
        }

        // GET: CategoriasMenu/Create
        [Authorize(Roles = "administrador,personal")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoriasMenu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCategoria,Nombre,Descripcion")] CategoriasMenu categoriasMenu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoriasMenu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoriasMenu);
        }

        // GET: CategoriasMenu/Edit/5
        [Authorize(Roles = "administrador,personal")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriasMenu = await _context.CategoriasMenus.FindAsync(id);
            if (categoriasMenu == null)
            {
                return NotFound();
            }
            return View(categoriasMenu);
        }

        // POST: CategoriasMenu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCategoria,Nombre,Descripcion")] CategoriasMenu categoriasMenu)
        {
            if (id != categoriasMenu.IdCategoria)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoriasMenu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriasMenuExists(categoriasMenu.IdCategoria))
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
            return View(categoriasMenu);
        }

        // GET: CategoriasMenu/Delete/5
        [Authorize(Roles = "administrador,personal")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriasMenu = await _context.CategoriasMenus
                .FirstOrDefaultAsync(m => m.IdCategoria == id);
            if (categoriasMenu == null)
            {
                return NotFound();
            }

            return View(categoriasMenu);
        }

        // POST: CategoriasMenu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoriasMenu = await _context.CategoriasMenus.FindAsync(id);
            if (categoriasMenu != null)
            {
                _context.CategoriasMenus.Remove(categoriasMenu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriasMenuExists(int id)
        {
            return _context.CategoriasMenus.Any(e => e.IdCategoria == id);
        }
    }
}
