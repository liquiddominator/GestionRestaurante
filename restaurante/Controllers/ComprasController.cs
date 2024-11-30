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
                .Include(c => c.IdUsuarioNavigation)
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
                .Include(c => c.IdUsuarioNavigation)
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
        public async Task<IActionResult> Create()
        {
            // Obtener usuarios con rol proveedor
            var proveedores = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => u.UsuarioRoles.Any(ur => ur.IdRolNavigation.Nombre == "proveedor"))
                .Select(u => new
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido} ({u.Email})"
                })
                .ToListAsync();

            ViewData["IdUsuario"] = new SelectList(proveedores, "IdUsuario", "NombreCompleto");
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
                    IdUsuario = viewModel.IdUsuario,
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

            var proveedores = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => u.UsuarioRoles.Any(ur => ur.IdRolNavigation.Nombre == "proveedor"))
                .Select(u => new
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido} ({u.Email})"
                })
                .ToListAsync();

            ViewData["IdUsuario"] = new SelectList(proveedores, "IdUsuario", "NombreCompleto", viewModel.IdUsuario);
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
                IdCompra = compra.IdCompra,
                IdUsuario = compra.IdUsuario.Value,
                FechaCompra = compra.FechaCompra,
                Total = compra.Total,
                Detalles = compra.DetallesCompras.Select(d => new DetalleCompraViewModel
                {
                    IdInventario = d.IdInventario.Value,
                    Cantidad = (int)d.Cantidad
                }).ToList()
            };

            var proveedores = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => u.UsuarioRoles.Any(ur => ur.IdRolNavigation.Nombre == "proveedor"))
                .Select(u => new
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido} ({u.Email})"
                })
                .ToListAsync();

            ViewData["IdUsuario"] = new SelectList(proveedores, "IdUsuario", "NombreCompleto", compra.IdUsuario);
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

                    compra.IdUsuario = viewModel.IdUsuario;
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

            var proveedores = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => u.UsuarioRoles.Any(ur => ur.IdRolNavigation.Nombre == "proveedor"))
                .Select(u => new
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido} ({u.Email})"
                })
                .ToListAsync();

            ViewData["IdUsuario"] = new SelectList(proveedores, "IdUsuario", "NombreCompleto", viewModel.IdUsuario);
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
                .Include(c => c.IdUsuarioNavigation)
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
        public int IdUsuario { get; set; }  // Cambiado de IdProveedor
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