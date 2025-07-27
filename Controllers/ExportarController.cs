using AgenciaDeTours.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AgenciaDeTours.Controllers
{
    public class ExportarController : Controller
    {
        private readonly AgenciaToursContext _context;

        public ExportarController(AgenciaToursContext context)
        {
            _context = context;
        }

        public IActionResult ToursCsv()
        {
            var tours = _context.Tours
                .Include(t => t.Pais)
                .Include(t => t.Destino)
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("ID,Nombre,Pais,Destino,Fecha,Hora,Precio,ITBIS,Duración,FechaHoraFin,Estado");

            foreach (var t in tours)
            {
                csv.AppendLine($"{t.TourId},{t.Nombre},{t.Pais?.Nombre},{t.Destino?.Nombre},{t.Fecha:yyyy-MM-dd},{t.Hora},{t.Precio},{t.Itbis},{t.DuracionTotalHoras},{t.FechaHoraFin},{t.Estado}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "tours.csv");
        }
    }
}
