using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restaurante.Models;

namespace restaurante.Controllers
{
    [Authorize(Roles = "administrador,personal,cliente")]
    public class CarritoController : Controller
    {
        private readonly RestauranteWebContext _context;

        public CarritoController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: Carrito/ObtenerCarrito
        [HttpGet]
        public async Task<IActionResult> ObtenerCarrito()
        {
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (currentUser == null)
            {
                return PartialView("_CarritoPartial", new CarritoViewModel());
            }

            var pedidosUsuario = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.IdItemNavigation)
                .Where(p => p.IdUsuario == currentUser.IdUsuario &&
                           (p.Estado == "pendiente" || p.Estado == "en_preparacion"))
                .OrderByDescending(p => p.FechaHora)
                .ToListAsync();

            var viewModel = new CarritoViewModel
            {
                Pedidos = pedidosUsuario,
                TotalCarrito = pedidosUsuario.Sum(p => p.Total)
            };

            return PartialView("_CarritoPartial", viewModel);
        }

        // GET: Carrito/ObtenerContador
        [HttpGet]
        public async Task<JsonResult> ObtenerContador()
        {
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (currentUser == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _context.Pedidos
                .CountAsync(p => p.IdUsuario == currentUser.IdUsuario &&
                                (p.Estado == "pendiente" || p.Estado == "en_preparacion"));

            return Json(new { count });
        }

        // POST: Carrito/EliminarPedido/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.DetallesPedidos)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // Verificar que el usuario actual sea el dueño del pedido
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (currentUser == null || pedido.IdUsuario != currentUser.IdUsuario)
            {
                return Forbid();
            }

            // Solo permitir eliminar pedidos pendientes o en preparación
            if (pedido.Estado != "pendiente" && pedido.Estado != "en_preparacion")
            {
                TempData["Error"] = "Solo se pueden eliminar pedidos pendientes o en preparación.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminar los detalles del pedido primero
            _context.DetallesPedidos.RemoveRange(pedido.DetallesPedidos);
            // Luego eliminar el pedido
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pedido eliminado correctamente.";
            return Json(new { success = true });
        }

        // POST: Carrito/EliminarDetalle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDetalle(int idDetalle, int idPedido)
        {
            var detalle = await _context.DetallesPedidos
                .Include(d => d.IdPedidoNavigation)
                .FirstOrDefaultAsync(d => d.IdDetalle == idDetalle);

            if (detalle == null)
            {
                return NotFound();
            }

            // Verificar que el usuario actual sea el dueño del pedido
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            if (currentUser == null || detalle.IdPedidoNavigation.IdUsuario != currentUser.IdUsuario)
            {
                return Forbid();
            }

            // Actualizar el total del pedido
            var pedido = detalle.IdPedidoNavigation;
            pedido.Total -= detalle.Subtotal;

            _context.DetallesPedidos.Remove(detalle);

            // Si era el último detalle, eliminar el pedido completo
            if (!await _context.DetallesPedidos.AnyAsync(d => d.IdPedido == idPedido && d.IdDetalle != idDetalle))
            {
                _context.Pedidos.Remove(pedido);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class CarritoViewModel
    {
        public List<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public decimal TotalCarrito { get; set; }
    }
}
