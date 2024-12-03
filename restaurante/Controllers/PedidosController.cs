using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restaurante.Models;
using System.Security.Claims;

namespace restaurante.Controllers
{
    [Authorize(Roles = "administrador,personal,cliente")]
    public class PedidosController : Controller
    {
        private readonly RestauranteWebContext _context;

        public PedidosController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            // Construimos la consulta base
            var query = _context.Pedidos.AsQueryable();

            // Aplicamos los includes
            query = query
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                .ThenInclude(d => d.IdItemNavigation);

            // Filtramos si es cliente
            if (User.IsInRole("cliente"))
            {
                query = query.Where(p => p.IdUsuario == currentUser.IdUsuario);
            }

            // Ejecutamos la consulta
            var pedidosConDetalles = await query.ToListAsync();

            // Preparamos los ViewData
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "Email");
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre");
            ViewData["CurrentUserId"] = currentUser?.IdUsuario;

            return View(pedidosConDetalles);
        }

        // GET: Pedidos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                .ThenInclude(d => d.IdItemNavigation)
                .FirstOrDefaultAsync(m => m.IdPedido == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // Verificar si el usuario es cliente y si el pedido le pertenece
            if (User.IsInRole("cliente"))
            {
                var currentUserEmail = User.Identity.Name;
                var currentUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (pedido.IdUsuario != currentUser.IdUsuario)
                {
                    return Forbid(); // O puedes redirigir a una página de acceso denegado
                }
            }

            return View(pedido);
        }

        // POST: Pedidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FechaHora,Estado,MetodoPago")] Pedido pedido, List<int> IdItems, List<int> Cantidades)
        {
            if (ModelState.IsValid)
            {
                var currentUserEmail = User.Identity.Name; // Asumiendo que el nombre de usuario es el email
                var currentUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                if (currentUser == null)
                {
                    // Manejar el caso de usuario no encontrado
                    ModelState.AddModelError(string.Empty, "No se pudo encontrar el usuario actual. Por favor, inicie sesión nuevamente.");
                    ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre");
                    return View(pedido);
                }

                pedido.IdUsuario = currentUser.IdUsuario;
                pedido.Total = 0;
                _context.Add(pedido);
                await _context.SaveChangesAsync();

                for (int i = 0; i < IdItems.Count; i++)
                {
                    var item = await _context.Items.FindAsync(IdItems[i]);
                    if (item != null)
                    {
                        var detalle = new DetallesPedido
                        {
                            IdPedido = pedido.IdPedido,
                            IdItem = IdItems[i],
                            Cantidad = Cantidades[i],
                            PrecioUnitario = item.Precio,
                            Subtotal = item.Precio * Cantidades[i]
                        };
                        _context.Add(detalle);
                        pedido.Total += detalle.Subtotal;
                    }
                }

                _context.Update(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre");
            return View(pedido);
        }

        // POST: Pedidos/CreateDetalle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDetalle([Bind("IdPedido,IdItem,Cantidad,Notas")] DetallesPedido detallePedido)
        {
            if (ModelState.IsValid)
            {
                var item = await _context.Items.FindAsync(detallePedido.IdItem);
                if (item != null)
                {
                    detallePedido.PrecioUnitario = item.Precio;
                    detallePedido.Subtotal = detallePedido.Cantidad * detallePedido.PrecioUnitario;
                    _context.Add(detallePedido);

                    var pedido = await _context.Pedidos.FindAsync(detallePedido.IdPedido);
                    if (pedido != null)
                    {
                        pedido.Total += detallePedido.Subtotal;
                        _context.Update(pedido);
                    }

                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Pedidos/EditDetalle/5
        public async Task<IActionResult> EditDetalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detallePedido = await _context.DetallesPedidos.FindAsync(id);
            if (detallePedido == null)
            {
                return NotFound();
            }
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", detallePedido.IdItem);
            return View(detallePedido);
        }

        // POST: Pedidos/EditDetalle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDetalle(int id, [Bind("IdDetalle,IdPedido,IdItem,Cantidad,Notas")] DetallesPedido detallePedido)
        {
            if (id != detallePedido.IdDetalle)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var item = await _context.Items.FindAsync(detallePedido.IdItem);
                    if (item != null)
                    {
                        detallePedido.PrecioUnitario = item.Precio;
                        detallePedido.Subtotal = detallePedido.Cantidad * detallePedido.PrecioUnitario;
                        _context.Update(detallePedido);

                        var pedido = await _context.Pedidos.Include(p => p.DetallesPedidos).FirstOrDefaultAsync(p => p.IdPedido == detallePedido.IdPedido);
                        if (pedido != null)
                        {
                            pedido.Total = pedido.DetallesPedidos.Sum(d => d.Subtotal);
                            _context.Update(pedido);
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetallesPedidoExists(detallePedido.IdDetalle))
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
            ViewData["IdItem"] = new SelectList(_context.Items, "IdItem", "Nombre", detallePedido.IdItem);
            return View(detallePedido);
        }

        // GET: Pedidos/Delete/5
        [Authorize(Roles = "administrador,personal")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdPedido == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "administrador,personal")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await _context.Pedidos.Include(p => p.DetallesPedidos).FirstOrDefaultAsync(p => p.IdPedido == id);
            if (pedido != null)
            {
                _context.DetallesPedidos.RemoveRange(pedido.DetallesPedidos);
                _context.Pedidos.Remove(pedido);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Pedidos/DeleteDetalle/5
        [HttpPost, ActionName("DeleteDetalle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDetalleConfirmed(int id)
        {
            var detallePedido = await _context.DetallesPedidos.FindAsync(id);
            if (detallePedido != null)
            {
                _context.DetallesPedidos.Remove(detallePedido);

                var pedido = await _context.Pedidos.FindAsync(detallePedido.IdPedido);
                if (pedido != null)
                {
                    pedido.Total -= detallePedido.Subtotal;
                    _context.Update(pedido);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.IdPedido == id);
        }

        private bool DetallesPedidoExists(int id)
        {
            return _context.DetallesPedidos.Any(e => e.IdDetalle == id);
        }
    }
}