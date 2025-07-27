using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTours.Models;
using Microsoft.Extensions.Logging;

namespace AgenciaDeTours.Controllers
{
    public class PaisController : Controller
    {
        private readonly AgenciaToursContext _context;
        private readonly ILogger<PaisController> _logger;

        public PaisController(AgenciaToursContext context, ILogger<PaisController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Pais
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pais.ToListAsync());
        }

        // GET: Pais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pai = await _context.Pais
                .FirstOrDefaultAsync(m => m.PaisId == id);
            if (pai == null)
            {
                return NotFound();
            }

            return View(pai);
        }

        // GET: Pais/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaisId,Nombre,IdiomaOficial,Moneda,Continente,Descripcion")] Pai pai)
        {
            if (ModelState.IsValid)
            {
                pai.CodigoIso = await GenerarCodigoIsoUnicoAsync(pai.Nombre);

                try
                {
                    _context.Add(pai);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear país");
                    ModelState.AddModelError("", "No se pudo crear el país. Intente nuevamente.");
                }
            }
            return View(pai);
        }

        // GET: Pais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pai = await _context.Pais.FindAsync(id);
            if (pai == null)
            {
                return NotFound();
            }
            return View(pai);
        }

        // POST: Pais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaisId,Nombre,CodigoIso,IdiomaOficial,Moneda,Continente,Descripcion")] Pai pai)
        {
            if (id != pai.PaisId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que el código no esté duplicado (excepto para este registro)
                    if (await _context.Pais.AnyAsync(p => p.CodigoIso == pai.CodigoIso && p.PaisId != pai.PaisId))
                    {
                        ModelState.AddModelError("CodigoIso", "Este código ISO ya está en uso por otro país");
                        return View(pai);
                    }

                    _context.Update(pai);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaiExists(pai.PaisId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al editar país");
                    ModelState.AddModelError("", "No se pudo actualizar el país. Verifique los datos.");
                }
            }
            return View(pai);
        }

        // GET: Pais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pai = await _context.Pais
                .FirstOrDefaultAsync(m => m.PaisId == id);
            if (pai == null)
            {
                return NotFound();
            }

            return View(pai);
        }

        // POST: Pais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pai = await _context.Pais.FindAsync(id);
            if (pai != null)
            {
                try
                {
                    _context.Pais.Remove(pai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al eliminar país");
                    ModelState.AddModelError("", "No se puede eliminar el país porque está siendo utilizado.");
                    return View("Delete", pai);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PaiExists(int id)
        {
            return _context.Pais.Any(e => e.PaisId == id);
        }

        private async Task<string> GenerarCodigoIsoUnicoAsync(string nombrePais)
        {
            // 1. Limpieza del nombre
            var nombreLimpio = new string(nombrePais
                .Replace("El ", "").Replace("La ", "").Replace("Los ", "").Replace("Las ", "")
                .Replace(" ", "").Replace("-", "").Replace("'", "")
                .Where(char.IsLetter)
                .ToArray());

            // 2. Generar código base (3 letras mayúsculas)
            string codigoBase;
            if (nombreLimpio.Length >= 3)
            {
                codigoBase = nombreLimpio[..3].ToUpper();
            }
            else if (nombreLimpio.Length > 0)
            {
                codigoBase = nombreLimpio.ToUpper().PadRight(3, 'X');
            }
            else
            {
                codigoBase = "PIS"; // Fallback
            }

            // 3. Verificar unicidad y generar variantes
            string codigoFinal = codigoBase;
            int contador = 1;

            while (await _context.Pais.AnyAsync(p => p.CodigoIso == codigoFinal))
            {
                if (contador <= 26) // Usar letras A-Z
                {
                    codigoFinal = codigoBase[..2] + (char)('A' + contador - 1);
                }
                else // Usar números
                {
                    codigoFinal = codigoBase[..2] + contador;
                }
                contador++;
            }

            return codigoFinal;
        }
    }
}