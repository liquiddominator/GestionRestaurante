using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restaurante.Models;
using iTextSharp.text.pdf.draw;
using restaurante.ViewModels;

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
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == currentUserEmail);

            // Construimos la consulta base
            var query = _context.Pedidos.AsQueryable();

            // Aplicamos los includes y el ordenamiento
            query = query
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                .ThenInclude(d => d.IdItemNavigation);

            // Filtramos si es cliente
            if (User.IsInRole("cliente"))
            {
                query = query.Where(p => p.IdUsuario == currentUser.IdUsuario);
            }

            // Aplicamos el ordenamiento después del filtrado
            var pedidosOrdenados = query.OrderByDescending(p => p.FechaHora);

            // Ejecutamos la consulta
            var pedidosConDetalles = await pedidosOrdenados.ToListAsync();

            ViewBag.Estados = new List<string> { "pendiente", "en_preparacion", "listo", "entregado", "cancelado" };
            ViewBag.IsCliente = User.IsInRole("cliente");

            return View(pedidosConDetalles);
        }

        // Controllers/DetallesPedidosController.cs - Método modificado
        [HttpGet]
[Authorize(Roles = "administrador,personal,cliente")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.DetallesPedidos)
                    .ThenInclude(d => d.IdItemNavigation)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null) return NotFound();

            var viewModel = new PdfViewModel
            {
                Pedido = pedido,
                LogoPath = "wwwroot/images/logo.png",
                CompanyName = "GOURMET",
                CompanyAddress = "Equipetrol",
                CompanyPhone = "77618892",
                CompanyEmail = "gourmetRes@gourmet.org"
            };

            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 45f, 45f, 60f, 60f);
                var writer = PdfWriter.GetInstance(document, ms);
                writer.PageEvent = new WatermarkPageEvent(viewModel.LogoPath);

                document.Open();

                // Nueva paleta de colores
                var colorPrimario = new BaseColor(139, 115, 85);     // Marrón principal
                var colorSecundario = new BaseColor(186, 167, 123);  // Dorado/Beige
                var colorFondo = new BaseColor(242, 239, 233);       // Beige claro
                var colorTexto = new BaseColor(89, 74, 55);         // Marrón oscuro
                var colorAccent = new BaseColor(164, 145, 96);      // Dorado accent

                // Fuentes actualizadas
                var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                var titleFont = new Font(baseFont, 22, Font.BOLD, colorTexto);
                var headerFont = new Font(baseFont, 14, Font.BOLD, colorTexto);
                var normalFont = new Font(baseFont, 11, Font.NORMAL, colorTexto);
                var smallFont = new Font(baseFont, 9, Font.NORMAL, colorSecundario);
                var accentFont = new Font(baseFont, 12, Font.BOLD, colorAccent);

                // Encabezado con logotipo centrado
                var headerTable = new PdfPTable(3) { WidthPercentage = 100 };
                headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
                headerTable.SetWidths(new float[] { 30, 40, 30 }); // Proporciones de las columnas

                // Celda vacía a la izquierda
                var emptyCellLeft = new PdfPCell(new Paragraph()) { Border = Rectangle.NO_BORDER };
                headerTable.AddCell(emptyCellLeft);

                // Celda central con el logotipo
                var logoImage = Image.GetInstance(viewModel.LogoPath);
                logoImage.ScaleToFit(100f, 100f); // Ajusta el tamaño del logotipo
                logoImage.Alignment = Element.ALIGN_CENTER;

                var logoCell = new PdfPCell(logoImage)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                headerTable.AddCell(logoCell);

                // Celda vacía a la derecha
                var emptyCellRight = new PdfPCell(new Paragraph()) { Border = Rectangle.NO_BORDER };
                headerTable.AddCell(emptyCellRight);

                document.Add(headerTable);

                // Información de la empresa y detalles de factura
                var companyHeader = new PdfPTable(2) { WidthPercentage = 100 };
                companyHeader.DefaultCell.Border = Rectangle.NO_BORDER;
                companyHeader.SetWidths(new float[] { 60, 40 });

                // Información de la empresa con fondo
                var companyCell = new PdfPCell(new Paragraph());
                companyCell.AddElement(new Chunk(viewModel.CompanyName + "\n", headerFont));
                companyCell.AddElement(new Chunk(viewModel.CompanyAddress + "\n", normalFont));
                companyCell.AddElement(new Chunk("Tel: " + viewModel.CompanyPhone + "\n", normalFont));
                companyCell.AddElement(new Chunk(viewModel.CompanyEmail, normalFont));
                companyCell.Border = Rectangle.NO_BORDER;
                companyCell.BackgroundColor = new BaseColor(colorFondo.R, colorFondo.G, colorFondo.B, 60); // 60% opacidad
                companyCell.PaddingLeft = 10f;
                companyCell.PaddingTop = 10f;
                companyCell.PaddingBottom = 10f;
                companyHeader.AddCell(companyCell);

                // Número de factura con fondo
                var invoiceCell = new PdfPCell(new Paragraph());
                var invoiceInfo = new Paragraph();
                invoiceInfo.Alignment = Element.ALIGN_RIGHT;
                invoiceInfo.Add(new Chunk("FACTURA\n", titleFont));
                invoiceInfo.Add(new Chunk($"#{pedido.IdPedido:D6}\n", accentFont));
                invoiceInfo.Add(new Chunk($"Fecha: {pedido.FechaHora:dd/MM/yyyy HH:mm}", smallFont));
                invoiceCell.AddElement(invoiceInfo);
                invoiceCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                invoiceCell.Border = Rectangle.NO_BORDER;
                invoiceCell.BackgroundColor = new BaseColor(colorFondo.R, colorFondo.G, colorFondo.B, 60);
                invoiceCell.PaddingRight = 10f;
                invoiceCell.PaddingTop = 10f;
                invoiceCell.PaddingBottom = 10f;
                companyHeader.AddCell(invoiceCell);

                document.Add(companyHeader);
                document.Add(new Paragraph("\n"));

                // Línea separadora
                var lineSeparator = new LineSeparator(1f, 100f, colorSecundario, Element.ALIGN_CENTER, -1);
                document.Add(lineSeparator);
                document.Add(new Paragraph("\n"));

                // Información del cliente con nuevo estilo
                var clienteTable = new PdfPTable(2) { WidthPercentage = 100 };
                clienteTable.DefaultCell.Border = Rectangle.NO_BORDER;
                clienteTable.SetWidths(new float[] { 50, 50 });

                // Cliente info con fondo
                var clienteCell = new PdfPCell();
                var clienteInfo = new Paragraph();
                clienteInfo.Add(new Chunk("DATOS DEL CLIENTE\n", headerFont));
                clienteInfo.Add(new Chunk($"{pedido.IdUsuarioNavigation.Nombre} {pedido.IdUsuarioNavigation.Apellido}\n", normalFont));
                clienteInfo.Add(new Chunk(pedido.IdUsuarioNavigation.Email, smallFont));
                clienteCell.AddElement(clienteInfo);
                clienteCell.Border = Rectangle.NO_BORDER;
                clienteCell.BackgroundColor = new BaseColor(colorFondo.R, colorFondo.G, colorFondo.B, 40);
                clienteCell.PaddingLeft = 10f;
                clienteCell.PaddingTop = 10f;
                clienteCell.PaddingBottom = 10f;
                clienteTable.AddCell(clienteCell);

                // Detalles de pago con fondo
                var paymentCell = new PdfPCell();
                var paymentInfo = new Paragraph();
                paymentInfo.Add(new Chunk("DETALLES DE PAGO\n", headerFont));
                paymentInfo.Add(new Chunk($"Método: {pedido.MetodoPago}\n", normalFont));
                paymentInfo.Add(new Chunk($"Estado: {pedido.Estado}", smallFont));
                paymentCell.AddElement(paymentInfo);
                paymentCell.Border = Rectangle.NO_BORDER;
                paymentCell.BackgroundColor = new BaseColor(colorFondo.R, colorFondo.G, colorFondo.B, 40);
                paymentCell.PaddingLeft = 10f;
                paymentCell.PaddingTop = 10f;
                paymentCell.PaddingBottom = 10f;
                clienteTable.AddCell(paymentCell);

                document.Add(clienteTable);
                document.Add(new Paragraph("\n"));

                // Tabla de detalles con nuevo estilo
                var table = new PdfPTable(5) { WidthPercentage = 100, SpacingBefore = 15f, SpacingAfter = 15f };
                table.SetWidths(new float[] { 40f, 15f, 15f, 15f, 15f });

                // Encabezados de tabla actualizados
                string[] headers = { "Ítem", "Cantidad", "P. Unitario", "Subtotal", "Notas" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, new Font(baseFont, 11, Font.BOLD, BaseColor.WHITE)))
                    {
                        BackgroundColor = colorPrimario,
                        PaddingTop = 8f,
                        PaddingBottom = 8f,
                        PaddingLeft = 8f,
                        PaddingRight = 8f,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                    table.AddCell(cell);
                }

                // Filas con colores alternados actualizados
                var count = 0;
                foreach (var detalle in pedido.DetallesPedidos)
                {
                    var rowColor = count++ % 2 == 0 ?
                        new BaseColor(colorFondo.R, colorFondo.G, colorFondo.B, 40) :
                        new BaseColor(255, 255, 255, 255);

                    var cells = new[]
                    {
                new { Text = detalle.IdItemNavigation.Nombre, Align = Element.ALIGN_LEFT },
                new { Text = detalle.Cantidad.ToString(), Align = Element.ALIGN_CENTER },
                new { Text = detalle.PrecioUnitario.ToString("C"), Align = Element.ALIGN_RIGHT },
                new { Text = detalle.Subtotal.ToString("C"), Align = Element.ALIGN_RIGHT },
                new { Text = detalle.Notas ?? "", Align = Element.ALIGN_LEFT }
            };

                    foreach (var cell in cells)
                    {
                        table.AddCell(new PdfPCell(new Phrase(cell.Text, normalFont))
                        {
                            BackgroundColor = rowColor,
                            Padding = 6f,
                            HorizontalAlignment = cell.Align,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        });
                    }
                }

                // Total actualizado
                var cellTotal = new PdfPCell(new Phrase("TOTAL:", headerFont))
                {
                    Colspan = 4,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 8f,
                    BackgroundColor = colorSecundario,
                    Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER
                };
                table.AddCell(cellTotal);

                var cellTotalValor = new PdfPCell(new Phrase(pedido.Total.ToString("C"), headerFont))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 8f,
                    BackgroundColor = colorSecundario,
                    Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER
                };
                table.AddCell(cellTotalValor);

                document.Add(table);

                // Pie de página actualizado
                document.Add(new Paragraph("\n"));
                var footer = new Paragraph();
                footer.Alignment = Element.ALIGN_CENTER;
                footer.Add(new Chunk("¡Gracias por su preferencia!\n", accentFont));
                footer.Add(new Chunk("Sus comentarios son importantes para nosotros", smallFont));
                document.Add(footer);

                document.Close();
                return File(ms.ToArray(), "application/pdf", $"Factura_Pedido_{pedido.IdPedido:D6}.pdf");
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