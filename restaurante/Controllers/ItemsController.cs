
using ClosedXML.Excel;
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

        // GET: Items/ExportToExcel
        [Authorize(Roles = "administrador,personal")]
        public async Task<IActionResult> ExportToExcel()
        {
            var items = await _context.Items
                .Include(i => i.IdCategoriaNavigation)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Menú Gourmet");

                // Cabeceras
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nombre";
                worksheet.Cell(1, 3).Value = "Descripción";
                worksheet.Cell(1, 4).Value = "Precio";
                worksheet.Cell(1, 5).Value = "Categoría";
                worksheet.Cell(1, 6).Value = "Disponible";
                worksheet.Cell(1, 7).Value = "Tiempo de Preparación";
                worksheet.Cell(1, 8).Value = "Imagen";

                // Estilos para las cabeceras
                var headerRow = worksheet.Range(1, 1, 1, 8);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Ruta base de las imágenes
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "menu");

                // Tamaño fijo para las imágenes
                const double imageWidth = 100;  // Ancho en puntos
                const double imageHeight = 60; // Alto en puntos

                // Ajuste de altura de fila
                const double rowHeight = 50; // Altura uniforme para las filas

                // Datos
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    int row = i + 2;

                    worksheet.Cell(row, 1).Value = item.IdItem;
                    worksheet.Cell(row, 2).Value = item.Nombre;
                    worksheet.Cell(row, 3).Value = item.Descripcion;
                    worksheet.Cell(row, 4).Value = item.Precio;
                    worksheet.Cell(row, 5).Value = item.IdCategoriaNavigation.Nombre;
                    worksheet.Cell(row, 6).Value = item.Disponible.HasValue ? (item.Disponible.Value ? "Sí" : "No") : "Consultar";
                    worksheet.Cell(row, 7).Value = item.TiempoPreparacion;

                    // Ajustar la altura de la fila
                    worksheet.Row(row).Height = rowHeight;

                    // Obtener la ruta completa de la imagen
                    var imagePath = Path.Combine(basePath, Path.GetFileName(item.ImagenUrl));

                    if (System.IO.File.Exists(imagePath))
                    {
                        using (var imageStream = System.IO.File.OpenRead(imagePath))
                        {
                            var picture = worksheet.AddPicture(imageStream); // Añadir la imagen
                            picture.MoveTo(worksheet.Cell(row, 8)); // Posicionar en la celda

                            // Redimensionar la imagen
                            picture.Width = (int)imageWidth;   // Convertir a int
                            picture.Height = (int)imageHeight; // Convertir a int
                        }
                    }
                    else
                    {
                        worksheet.Cell(row, 8).Value = "Imagen no encontrada";
                    }
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                // Ajustar el ancho de la columna de imágenes
                worksheet.Column(8).Width = 20; // Ajusta este valor según el tamaño deseado

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "MenuGourmet.xlsx");
                }
            }
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
