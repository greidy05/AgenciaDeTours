using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTours.Models;

namespace AgenciaDeTours.Controllers
{
    public class ToursController : Controller
    {
        private readonly AgenciaToursContext _context;

        public ToursController(AgenciaToursContext context)
        {
            _context = context;
        }

        // GET: Tours
        public async Task<IActionResult> Index()
        {
            var tours = await _context.Tours
                .Include(t => t.Destino)
                .Include(t => t.Pais)
                .OrderBy(t => t.Fecha)
                .ThenBy(t => t.Hora)
                .ToListAsync();

            return View(tours);
        }

        // GET: Tours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.Destino)
                .Include(t => t.Pais)
                .FirstOrDefaultAsync(m => m.TourId == id);

            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // GET: Tours/Create
        public async Task<IActionResult> Create()
        {
            await CargarListasDesplegables();
            return View(new Tour
            {
                Fecha = DateOnly.FromDateTime(DateTime.Today),
                Hora = TimeOnly.FromDateTime(DateTime.Now),
                DuracionTotalHoras = 1
            });
        }

        // POST: Tours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TourId,Nombre,PaisId,DestinoId,Fecha,Hora,Precio,DuracionTotalHoras")] Tour tour)
        {
            if (ModelState.IsValid)
            {
                // Calcular FechaHoraFin
                var fechaHoraInicio = tour.Fecha.ToDateTime(tour.Hora);
                tour.FechaHoraFin = fechaHoraInicio.AddHours(tour.DuracionTotalHoras);

                // Calcular ITBIS (18%)
                tour.Itbis = tour.Precio * 0.18m;

                // Calcular estado
                tour.Estado = tour.FechaHoraFin >= DateTime.Now ? "Vigente" : "No Vigente";

                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CargarListasDesplegables(tour.PaisId, tour.DestinoId);
            return View(tour);
        }

        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            await CargarListasDesplegables(tour.PaisId, tour.DestinoId);
            return View(tour);
        }

        // POST: Tours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TourId,Nombre,PaisId,DestinoId,Fecha,Hora,Precio,DuracionTotalHoras,FechaHoraFin,Itbis,Estado")] Tour tour)
        {
            if (id != tour.TourId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalcular FechaHoraFin si cambió la duración
                    var fechaHoraInicio = tour.Fecha.ToDateTime(tour.Hora);
                    tour.FechaHoraFin = fechaHoraInicio.AddHours(tour.DuracionTotalHoras);

                    // Recalcular ITBIS y estado
                    tour.Itbis = tour.Precio * 0.18m;
                    tour.Estado = tour.FechaHoraFin >= DateTime.Now ? "Vigente" : "No Vigente";

                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.TourId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await CargarListasDesplegables(tour.PaisId, tour.DestinoId);
            return View(tour);
        }

        // GET: Tours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.Destino)
                .Include(t => t.Pais)
                .FirstOrDefaultAsync(m => m.TourId == id);

            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Tours/GetDestinosByPais
        [HttpGet]
        public async Task<IActionResult> GetDestinosByPais(int paisId)
        {
            var destinos = await _context.Destinos
                .Where(d => d.PaisId == paisId)
                .Select(d => new {
                    value = d.DestinoId,
                    text = d.Nombre
                })
                .ToListAsync();

            return Json(destinos);
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.TourId == id);
        }

        private async Task CargarListasDesplegables(int? paisId = null, int? destinoId = null)
        {
            ViewData["Paises"] = new SelectList(
                await _context.Pais.OrderBy(p => p.Nombre).ToListAsync(),
                "PaisId",
                "Nombre",
                paisId);

            var destinosQuery = _context.Destinos.AsQueryable();

            if (paisId.HasValue)
            {
                destinosQuery = destinosQuery.Where(d => d.PaisId == paisId);
            }

            ViewData["Destinos"] = new SelectList(
                await destinosQuery.OrderBy(d => d.Nombre).ToListAsync(),
                "DestinoId",
                "Nombre",
                destinoId);
        }
    }
}