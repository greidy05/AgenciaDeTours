using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AgenciaDeTours.Models;
using Microsoft.Data.SqlClient;

namespace AgenciaDeTours.Controllers
{
    public class DestinoController : Controller
    {
        private readonly AgenciaToursContext _context;
        private readonly ILogger<DestinoController> _logger;

        public DestinoController(AgenciaToursContext context, ILogger<DestinoController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: Destino
        public async Task<IActionResult> Index()
        {
            try
            {
                var destinos = await _context.Destinos
                    .Include(d => d.Pais)
                    .AsNoTracking()
                    .OrderBy(d => d.Nombre)
                    .ToListAsync();
                return View(destinos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de destinos");
                TempData["ErrorMessage"] = "Error al cargar los destinos. Por favor, intente nuevamente.";
                return View(Enumerable.Empty<Destino>().ToList());
            }
        }

        // GET: Destino/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de destino no especificado.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var destino = await _context.Destinos
                    .Include(d => d.Pais)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.DestinoId == id);
                if (destino == null)
                {
                    TempData["ErrorMessage"] = "Destino no encontrado.";
                    return RedirectToAction(nameof(Index));
                }
                return View(destino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al mostrar detalles del destino ID: {id}");
                TempData["ErrorMessage"] = $"Error al cargar los detalles del destino ID: {id}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Destino/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadPaisesViewData();
                return View(new Destino());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el formulario de creación");
                TempData["ErrorMessage"] = "Error al cargar el formulario de creación.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Destino/Create
        // Se ha corregido el manejo de la transacción y se han añadido más logs.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,PaisId,DuracionDias,DuracionHoras,Clima,TipoDestino,Actividades,AtractivoPrincipal,Descripcion")] Destino destino)
        {
            _logger.LogInformation("Iniciando creación de destino: {Nombre}", destino.Nombre);
            try
            {
                // Validación adicional de duración
                if (destino.DuracionDias == 0 && destino.DuracionHoras == 0)
                {
                    _logger.LogWarning("Validación fallida: Duración total es cero para el destino {Nombre}", destino.Nombre);
                    ModelState.AddModelError("", "La duración total debe ser mayor a cero");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogInformation("ModelState no es válido. Mostrando vista Create nuevamente.");
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            _logger.LogInformation("Error de ModelState en {Key}: {ErrorMessage}", key, string.Join(", ", state.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                    await LoadPaisesViewData();
                    return View(destino);
                }

                // Verificar que el país existe (validación de integridad referencial)
                var pais = await _context.Pais.FindAsync(destino.PaisId);
                if (pais == null)
                {
                    _logger.LogWarning("País con ID {PaisId} no encontrado.", destino.PaisId);
                    ModelState.AddModelError("PaisId", "El país seleccionado no existe");
                    await LoadPaisesViewData();
                    return View(destino);
                }
                // Asignar la navegación completa (opcional, pero ayuda si se necesita después)
                destino.Pais = pais;

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Destinos.Add(destino);
                        _logger.LogInformation("Destino añadido al contexto. Guardando cambios...");
                        var result = await _context.SaveChangesAsync();
                        _logger.LogInformation("SaveChangesAsync devolvió {Result}", result);

                        if (result > 0)
                        {
                            await transaction.CommitAsync();
                            _logger.LogInformation("Transacción confirmada. Destino creado exitosamente.");
                            TempData["SuccessMessage"] = $"Destino '{destino.Nombre}' creado exitosamente!";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            _logger.LogWarning("SaveChangesAsync devolvió 0. No se confirmó la transacción.");
                            ModelState.AddModelError("", "No se pudo guardar el destino. No se detectaron cambios para guardar.");
                            await transaction.RollbackAsync(); // Explícito
                        }
                    }
                    catch (Exception ex) // Captura general dentro de la transacción
                    {
                        _logger.LogError(ex, "Excepción dentro de la transacción al crear destino.");
                        await transaction.RollbackAsync();
                        throw; // Relanza para que lo capture el catch externo
                    }
                }
                // Si llegamos aquí, hubo un problema (result == 0). Volver a la vista.
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al crear destino");
                ModelState.AddModelError("", GetDetailedDbError(dbEx));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear destino");
                TempData["ErrorMessage"] = "Error inesperado al crear el destino";
                return RedirectToAction(nameof(Index));
            }
            await LoadPaisesViewData();
            return View(destino);
        }


        // GET: Destino/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de destino no especificado.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var destino = await _context.Destinos.FindAsync(id);
                if (destino == null)
                {
                    TempData["ErrorMessage"] = $"Destino con ID {id} no encontrado.";
                    return RedirectToAction(nameof(Index));
                }
                await LoadPaisesViewData(destino.PaisId);
                return View(destino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar el formulario de edición para el destino ID: {id}");
                TempData["ErrorMessage"] = $"Error al cargar el formulario de edición para el destino ID: {id}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Destino/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DestinoId,Nombre,PaisId,DuracionDias,DuracionHoras,Clima,TipoDestino,Actividades,AtractivoPrincipal,Descripcion")] Destino destino)
        {
            if (id != destino.DestinoId)
            {
                TempData["ErrorMessage"] = "IDs no coinciden";
                return RedirectToAction(nameof(Index));
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        // Verificar que el país existe
                        if (!await _context.Pais.AnyAsync(p => p.PaisId == destino.PaisId))
                        {
                            ModelState.AddModelError("PaisId", "El país seleccionado no existe");
                            await LoadPaisesViewData();
                            return View(destino);
                        }
                        _context.Update(destino);
                        var result = await _context.SaveChangesAsync();
                        if (result > 0)
                        {
                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = "Destino actualizado correctamente";
                            return RedirectToAction(nameof(Index));
                        }
                        ModelState.AddModelError("", "No se realizaron cambios en la base de datos");
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    if (!await DestinoExistsAsync(destino.DestinoId))
                    {
                        TempData["ErrorMessage"] = "El destino ya no existe";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError(ex, $"Error de concurrencia al editar destino ID: {id}");
                        ModelState.AddModelError("", "El registro fue modificado por otro usuario. Recargue y vuelva a intentar.");
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, $"Error de BD al editar destino ID: {id}");
                    ModelState.AddModelError("", GetDetailedDbError(dbEx));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error al editar destino ID: {id}");
                    TempData["ErrorMessage"] = "Error al actualizar el destino";
                }
            }
            await LoadPaisesViewData(destino.PaisId);
            return View(destino);
        }

        // GET: Destino/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de destino no especificado.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var destino = await _context.Destinos
                    .Include(d => d.Pais)
                    .FirstOrDefaultAsync(m => m.DestinoId == id);
                if (destino == null)
                {
                    TempData["ErrorMessage"] = $"Destino con ID {id} no encontrado.";
                    return RedirectToAction(nameof(Index));
                }
                return View(destino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar el formulario de eliminación para el destino ID: {id}");
                TempData["ErrorMessage"] = $"Error al cargar el formulario de eliminación para el destino ID: {id}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Destino/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var destino = await _context.Destinos.FindAsync(id);
                    if (destino == null)
                    {
                        TempData["ErrorMessage"] = $"Destino con ID {id} no encontrado.";
                        return RedirectToAction(nameof(Index));
                    }
                    _context.Destinos.Remove(destino);
                    var result = await _context.SaveChangesAsync();
                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = $"Destino '{destino.Nombre}' eliminado exitosamente!";
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        TempData["WarningMessage"] = "No se pudo eliminar el destino. Ningún registro afectado.";
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error al eliminar el destino ID: {id}");
                    TempData["ErrorMessage"] = $"No se pudo eliminar el destino: {GetDetailedDbError(ex)}";
                    return RedirectToAction(nameof(Delete), new { id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error inesperado al eliminar el destino ID: {id}");
                    TempData["ErrorMessage"] = $"Error inesperado al eliminar el destino: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        private async Task<bool> DestinoExistsAsync(int id)
        {
            return await _context.Destinos.AnyAsync(e => e.DestinoId == id);
        }

        private async Task LoadPaisesViewData(object selectedValue = null)
        {
            try
            {
                var paises = await _context.Pais
                    .OrderBy(p => p.Nombre)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Paises = new SelectList(paises, "PaisId", "Nombre", selectedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de países");
                ViewBag.Paises = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private string GetDetailedDbError(DbUpdateException dbEx)
        {
            var sqlEx = dbEx.InnerException as SqlException;
            if (sqlEx != null)
            {
                return sqlEx.Number switch
                {
                    547 => "Error: No se puede completar la operación debido a restricciones de base de datos",
                    2601 or 2627 => "Error: Ya existe un registro con estos datos",
                    515 => "Error: Campos requeridos no pueden estar vacíos",
                    _ => $"Error de base de datos (Código: {sqlEx.Number})"
                };
            }
            return dbEx.InnerException?.Message ?? dbEx.Message;
        }

        [HttpGet]
        public async Task<IActionResult> CheckDatabaseConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var destinosCount = await _context.Destinos.CountAsync();
                var paisesCount = await _context.Pais.CountAsync();
                return Json(new
                {
                    Success = true,
                    DatabaseConnected = canConnect,
                    DestinosCount = destinosCount,
                    PaisesCount = paisesCount,
                    TablesExist = new
                    {
                        Destinos = _context.Model.FindEntityType(typeof(Destino)) != null,
                        Pais = _context.Model.FindEntityType(typeof(Pai)) != null
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}