using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index()
        {
            var pedidosConDetalles = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.IdItemNavigation)
                .OrderByDescending(p => p.FechaHora)
                .ToListAsync();

            ViewBag.Estados = new List<string> { "pendiente", "en_preparacion", "listo", "entregado", "cancelado" };
            return View(pedidosConDetalles);
        }

        [HttpGet]
        [Authorize(Roles = "administrador,personal,cliente")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            // Buscar el pedido por su ID con las relaciones necesarias
            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.IdItemNavigation)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null) return NotFound();

            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Encabezado
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                document.Add(new Paragraph($"Factura del Pedido #{pedido.IdPedido}", titleFont));
                document.Add(new Paragraph("\n"));

                // Información del cliente
                document.Add(new Paragraph($"Cliente: {pedido.IdUsuarioNavigation.Nombre} {pedido.IdUsuarioNavigation.Apellido}", normalFont));
                document.Add(new Paragraph($"Fecha: {pedido.FechaHora:dd/MM/yyyy HH:mm}", normalFont));
                document.Add(new Paragraph($"Método de Pago: {pedido.MetodoPago}", normalFont));
                document.Add(new Paragraph($"Estado: {pedido.Estado}", normalFont));
                document.Add(new Paragraph("\n"));

                // Tabla de detalles del pedido
                var table = new PdfPTable(4) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("Ítem", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("Cantidad", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("P. Unitario", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase("Subtotal", titleFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (var detalle in pedido.DetallesPedidos)
                {
                    table.AddCell(new PdfPCell(new Phrase(detalle.IdItemNavigation.Nombre, normalFont)));
                    table.AddCell(new PdfPCell(new Phrase(detalle.Cantidad.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(detalle.PrecioUnitario.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase(detalle.Subtotal.ToString("C"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                // Total
                table.AddCell(new PdfPCell(new Phrase("")) { Colspan = 2, Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase("Total:", titleFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(pedido.Total.ToString("C"), titleFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"Pedido_{pedido.IdPedido}.pdf");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.IdItemNavigation)
                .FirstOrDefaultAsync(m => m.IdPedido == id);
            if (pedido == null) return NotFound();
            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "personal")]
        public async Task<IActionResult> UpdateEstado(int id, string estado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = estado;
            _context.Update(pedido);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}