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
    public class ComprasController : Controller
    {
        private readonly RestauranteWebContext _context;

        public ComprasController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: Compras
        public async Task<IActionResult> Index()
        {
            var compras = await _context.Compras
                .Include(c => c.IdProveedorNavigation)
                .Include(c => c.DetallesCompras)
                    .ThenInclude(dc => dc.IdInventarioNavigation)
                .ToListAsync();
            return View(compras);
        }

        // GET: Compras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.Compras
                .Include(c => c.IdProveedorNavigation)
                .Include(c => c.DetallesCompras)
                    .ThenInclude(dc => dc.IdInventarioNavigation)
                .FirstOrDefaultAsync(m => m.IdCompra == id);
            if (compra == null)
            {
                return NotFound();
            }

            return View(compra);
        }

        // GET: Compras/Create
        public IActionResult Create()
        {
            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "Email");
            ViewData["Inventarios"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre");
            return View(new CompraViewModel { FechaCompra = DateTime.Now });
        }

        // POST: Compras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompraViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var compra = new Compra
                {
                    IdProveedor = viewModel.IdProveedor,
                    FechaCompra = viewModel.FechaCompra,
                    Total = 0
                };

                _context.Add(compra);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach (var detalle in viewModel.Detalles)
                {
                    var inventario = await _context.Inventarios.FindAsync(detalle.IdInventario);
                    if (inventario != null)
                    {
                        var detalleCompra = new DetallesCompra
                        {
                            IdCompra = compra.IdCompra,
                            IdInventario = detalle.IdInventario,
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = inventario.PrecioUnitario
                        };
                        _context.Add(detalleCompra);
                        total += detalleCompra.Cantidad * detalleCompra.PrecioUnitario;

                        inventario.Cantidad += detalle.Cantidad;
                        _context.Update(inventario);
                    }
                }

                compra.Total = total;
                _context.Update(compra);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "Email", viewModel.IdProveedor);
            ViewData["Inventarios"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre");
            return View(viewModel);
        }

        // GET: Compras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.Compras
                .Include(c => c.DetallesCompras)
                .FirstOrDefaultAsync(c => c.IdCompra == id);
            if (compra == null)
            {
                return NotFound();
            }

            var viewModel = new CompraViewModel
            {
                IdProveedor = compra.IdProveedor.Value,
                FechaCompra = compra.FechaCompra,
                Total = compra.Total,
                Detalles = compra.DetallesCompras.Select(d => new DetalleCompraViewModel
                {
                    IdInventario = d.IdInventario.Value,
                    Cantidad = (int)d.Cantidad
                }).ToList()
            };

            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "Email", compra.IdProveedor);
            ViewData["Inventarios"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre");
            return View(viewModel);
        }

        // POST: Compras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompraViewModel viewModel)
        {
            if (id != viewModel.IdCompra)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var compra = await _context.Compras
                        .Include(c => c.DetallesCompras)
                        .FirstOrDefaultAsync(c => c.IdCompra == id);

                    if (compra == null)
                    {
                        return NotFound();
                    }

                    compra.IdProveedor = viewModel.IdProveedor;
                    compra.FechaCompra = viewModel.FechaCompra;

                    // Revertir cambios en inventario
                    foreach (var detalle in compra.DetallesCompras)
                    {
                        var inventario = await _context.Inventarios.FindAsync(detalle.IdInventario);
                        if (inventario != null)
                        {
                            inventario.Cantidad -= detalle.Cantidad;
                            _context.Update(inventario);
                        }
                    }

                    // Eliminar detalles existentes
                    _context.DetallesCompras.RemoveRange(compra.DetallesCompras);

                    decimal total = 0;
                    foreach (var detalle in viewModel.Detalles)
                    {
                        var inventario = await _context.Inventarios.FindAsync(detalle.IdInventario);
                        if (inventario != null)
                        {
                            var detalleCompra = new DetallesCompra
                            {
                                IdCompra = compra.IdCompra,
                                IdInventario = detalle.IdInventario,
                                Cantidad = detalle.Cantidad,
                                PrecioUnitario = inventario.PrecioUnitario
                            };
                            _context.Add(detalleCompra);
                            total += detalleCompra.Cantidad * detalleCompra.PrecioUnitario;

                            inventario.Cantidad += detalle.Cantidad;
                            _context.Update(inventario);
                        }
                    }

                    compra.Total = total;
                    _context.Update(compra);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompraExists(viewModel.IdCompra))
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
            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "Email", viewModel.IdProveedor);
            ViewData["Inventarios"] = new SelectList(_context.Inventarios, "IdInventario", "Nombre");
            return View(viewModel);
        }

        // GET: Compras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compra = await _context.Compras
                .Include(c => c.IdProveedorNavigation)
                .Include(c => c.DetallesCompras)
                .FirstOrDefaultAsync(m => m.IdCompra == id);
            if (compra == null)
            {
                return NotFound();
            }

            return View(compra);
        }

        // POST: Compras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.DetallesCompras)
                .FirstOrDefaultAsync(c => c.IdCompra == id);
            if (compra != null)
            {
                // Revertir cambios en inventario
                foreach (var detalle in compra.DetallesCompras)
                {
                    var inventario = await _context.Inventarios.FindAsync(detalle.IdInventario);
                    if (inventario != null)
                    {
                        inventario.Cantidad -= detalle.Cantidad;
                        _context.Update(inventario);
                    }
                }

                _context.DetallesCompras.RemoveRange(compra.DetallesCompras);
                _context.Compras.Remove(compra);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompraExists(int id)
        {
            return _context.Compras.Any(e => e.IdCompra == id);
        }
    }

    public class CompraViewModel
    {
        public int IdCompra { get; set; }
        public int IdProveedor { get; set; }
        public DateTime FechaCompra { get; set; }
        public List<DetalleCompraViewModel> Detalles { get; set; } = new List<DetalleCompraViewModel>();
        public decimal Total { get; set; }
    }

    public class DetalleCompraViewModel
    {
        public int IdInventario { get; set; }
        public int Cantidad { get; set; }
    }
}