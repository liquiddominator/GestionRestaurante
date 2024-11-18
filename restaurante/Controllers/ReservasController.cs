using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using restaurante.Models;

namespace restaurante.Controllers
{
    [Authorize(Roles = "administrador,personal,cliente")]
    public class ReservasController : Controller
    {
        private readonly RestauranteWebContext _context;

        public ReservasController(RestauranteWebContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var viewModel = new ReservasMesasViewModel
            {
                Reservas = await _context.Reservas
                    .Include(r => r.IdMesaNavigation)
                    .Include(r => r.IdUsuarioNavigation)
                    .ToListAsync(),
                Mesas = await _context.Mesas.ToListAsync()
            };
            return View(viewModel);
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdMesaNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario.ToString() == userId);

            ViewData["IdMesa"] = new SelectList(_context.Mesas, "IdMesa", "Numero");
            ViewData["IdUsuario"] = user?.IdUsuario;
            ViewData["UserEmail"] = user?.Email;
            return View();
        }

        // POST: Reservas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReserva,IdMesa,FechaHora,NumeroPersonas,Estado")] Reserva reserva)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                reserva.IdUsuario = int.Parse(userId);

                var mesa = await _context.Mesas.FindAsync(reserva.IdMesa);
                if (mesa != null && reserva.NumeroPersonas > mesa.Capacidad)
                {
                    ModelState.AddModelError("NumeroPersonas", "El número de personas excede la capacidad de la mesa.");
                    ViewData["IdMesa"] = new SelectList(_context.Mesas, "IdMesa", "Numero", reserva.IdMesa);
                    ViewData["IdUsuario"] = reserva.IdUsuario;
                    ViewData["UserEmail"] = _context.Usuarios.Find(reserva.IdUsuario)?.Email;
                    return View(reserva);
                }

                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdMesa"] = new SelectList(_context.Mesas, "IdMesa", "Numero", reserva.IdMesa);
            ViewData["IdUsuario"] = reserva.IdUsuario;
            ViewData["UserEmail"] = _context.Usuarios.Find(reserva.IdUsuario)?.Email;
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            ViewData["IdMesa"] = new SelectList(_context.Mesas, "IdMesa", "Numero", reserva.IdMesa);
            ViewData["IdUsuario"] = reserva.IdUsuario;
            ViewData["UserEmail"] = _context.Usuarios.Find(reserva.IdUsuario)?.Email;
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReserva,IdMesa,IdUsuario,FechaHora,NumeroPersonas,Estado")] Reserva reserva)
        {
            if (id != reserva.IdReserva)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.IdReserva))
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
            ViewData["IdMesa"] = new SelectList(_context.Mesas, "IdMesa", "Numero", reserva.IdMesa);
            ViewData["IdUsuario"] = reserva.IdUsuario;
            ViewData["UserEmail"] = _context.Usuarios.Find(reserva.IdUsuario)?.Email;
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdMesaNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReserva == id);
        }

        // Métodos para Mesas
        public async Task<IActionResult> CreateMesa()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMesa([Bind("IdMesa,Numero,Capacidad,Estado")] Mesa mesa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mesa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mesa);
        }

        public async Task<IActionResult> EditMesa(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null)
            {
                return NotFound();
            }
            return View(mesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMesa(int id, [Bind("IdMesa,Numero,Capacidad,Estado")] Mesa mesa)
        {
            if (id != mesa.IdMesa)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mesa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MesaExists(mesa.IdMesa))
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
            return View(mesa);
        }

        public async Task<IActionResult> DeleteMesa(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.IdMesa == id);
            if (mesa == null)
            {
                return NotFound();
            }

            return View(mesa);
        }

        [HttpPost, ActionName("DeleteMesa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMesaConfirmed(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa != null)
            {
                _context.Mesas.Remove(mesa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MesaExists(int id)
        {
            return _context.Mesas.Any(e => e.IdMesa == id);
        }
    }

    public class ReservasMesasViewModel
    {
        public List<Reserva> Reservas { get; set; }
        public List<Mesa> Mesas { get; set; }
    }
}
