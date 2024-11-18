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
    public class DetallesPedidosController : Controller
    {
        private readonly RestauranteWebContext _context;

        public DetallesPedidosController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: DetallesPedidos
        public async Task<IActionResult> Index()
        {
            var restauranteWebContext = _context.DetallesPedidos.Include(d => d.IdItemNavigation).Include(d => d.IdPedidoNavigation);
            return View(await restauranteWebContext.ToListAsync());
        }

        // GET: DetallesPedidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallesPedido = await _context.DetallesPedidos
                .Include(d => d.IdItemNavigation)
                .Include(d => d.IdPedidoNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalle == id);
            if (detallesPedido == null)
            {
                return NotFound();
            }

            return View(detallesPedido);
        }

        // GET: DetallesPedidos/Create
        public IActionResult Create()
        {
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre");
            ViewData["IdPedido"] = new SelectList(_context.Pedidos, "IdPedido", "IdPedido");
            return View();
        }

        // POST: DetallesPedidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDetalle,IdPedido,IdItem,Cantidad,PrecioUnitario,Notas")] DetallesPedido detallesPedido)
        {
            var existeDetalle = await _context.DetallesPedidos
                .AnyAsync(d => d.IdPedido == detallesPedido.IdPedido && d.IdItem == detallesPedido.IdItem);

            if (existeDetalle)
            {
                ModelState.AddModelError("", "Este pedido ya contiene un detalle con el mismo ítem.");
                ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", detallesPedido.IdItem);
                ViewData["IdPedido"] = new SelectList(_context.Pedidos, "IdPedido", "IdPedido", detallesPedido.IdPedido);
                return View(detallesPedido);
            }

            if (ModelState.IsValid)
            {
                // Calcular el subtotal
                detallesPedido.Subtotal = detallesPedido.Cantidad * detallesPedido.PrecioUnitario;

                _context.Add(detallesPedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", detallesPedido.IdItem);
            ViewData["IdPedido"] = new SelectList(_context.Pedidos, "IdPedido", "IdPedido", detallesPedido.IdPedido);
            return View(detallesPedido);
        }



        // GET: DetallesPedidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallesPedido = await _context.DetallesPedidos.FindAsync(id);
            if (detallesPedido == null)
            {
                return NotFound();
            }
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", detallesPedido.IdItem);
            ViewData["IdPedido"] = new SelectList(_context.Pedidos, "IdPedido", "IdPedido", detallesPedido.IdPedido);
            return View(detallesPedido);
        }

        // POST: DetallesPedidos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDetalle,IdPedido,IdItem,Cantidad,PrecioUnitario,Notas")] DetallesPedido detallesPedido)
        {
            if (id != detallesPedido.IdDetalle)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Calcular el subtotal
                    detallesPedido.Subtotal = detallesPedido.Cantidad * detallesPedido.PrecioUnitario;

                    _context.Update(detallesPedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetallesPedidoExists(detallesPedido.IdDetalle))
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
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", detallesPedido.IdItem);
            ViewData["IdPedido"] = new SelectList(_context.Pedidos, "IdPedido", "IdPedido", detallesPedido.IdPedido);
            return View(detallesPedido);
        }

        // GET: DetallesPedidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallesPedido = await _context.DetallesPedidos
                .Include(d => d.IdItemNavigation)
                .Include(d => d.IdPedidoNavigation)
                .FirstOrDefaultAsync(m => m.IdDetalle == id);
            if (detallesPedido == null)
            {
                return NotFound();
            }

            return View(detallesPedido);
        }

        // POST: DetallesPedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detallesPedido = await _context.DetallesPedidos.FindAsync(id);
            if (detallesPedido != null)
            {
                _context.DetallesPedidos.Remove(detallesPedido);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetallesPedidoExists(int id)
        {
            return _context.DetallesPedidos.Any(e => e.IdDetalle == id);
        }
    }
}
