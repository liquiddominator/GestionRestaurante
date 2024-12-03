using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restaurante.Models;

namespace restaurante.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly RestauranteWebContext _context;

        public DashboardController(RestauranteWebContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener datos para los gráficos
            var pedidosMes = await _context.Pedidos
                .Where(p => p.FechaHora >= DateTime.Now.AddDays(-30))
                .GroupBy(p => p.FechaHora.Date)
                .Select(g => new {
                    Fecha = g.Key.ToString("dd/MM"),
                    Total = g.Sum(p => p.Total)
                })
                .ToListAsync();

            var totalMesas = await _context.Mesas.CountAsync();
            var mesasOcupadas = await _context.Mesas
                .Where(m => m.Estado == "ocupada" || m.Reservas.Any(r =>
                    r.FechaHora.Date == DateTime.Today &&
                    r.Estado == "confirmada"))
                .CountAsync();

            // Top platos más vendidos
            var topPlatos = await _context.DetallesPedidos
                .Include(d => d.IdItemNavigation)
                .GroupBy(d => d.IdItemNavigation.Nombre)
                .Select(g => new {
                    Nombre = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToListAsync();

            // Datos para gráfico de compras
            var comprasMes = await _context.Compras
                .Where(c => c.FechaCompra >= DateTime.Now.AddDays(-30))
                .GroupBy(c => c.FechaCompra.Date)
                .Select(g => new {
                    Fecha = g.Key.ToString("dd/MM"),
                    Total = g.Sum(c => c.Total)
                })
                .ToListAsync();

            // Top 3 productos más comprados
            var topProductosComprados = await _context.DetallesCompras
                .Include(d => d.IdInventarioNavigation)
                .GroupBy(d => d.IdInventarioNavigation.Nombre)
                .Select(g => new {
                    Nombre = g.Key,
                    CantidadTotal = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.CantidadTotal)
                .Take(3)
                .ToListAsync();

            ViewBag.FechasPedidos = pedidosMes.Select(p => p.Fecha).ToList();
            ViewBag.TotalesPedidos = pedidosMes.Select(p => p.Total).ToList();
            ViewBag.OcupacionMesas = new int[] { mesasOcupadas, totalMesas - mesasOcupadas };
            ViewBag.NombresPlatos = topPlatos.Select(p => p.Nombre).ToList();
            ViewBag.CantidadesPlatos = topPlatos.Select(p => p.Cantidad).ToList();
            ViewBag.FechasCompras = comprasMes.Select(c => c.Fecha).ToList();
            ViewBag.TotalesCompras = comprasMes.Select(c => c.Total).ToList();
            ViewBag.NombresProductosComprados = topProductosComprados.Select(p => p.Nombre).ToList();
            ViewBag.CantidadesProductosComprados = topProductosComprados.Select(p => p.CantidadTotal).ToList();

            return View();
        }
    }
}