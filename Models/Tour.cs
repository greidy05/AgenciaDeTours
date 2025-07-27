using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciaDeTours.Models
{
    public class Tour
    {
        [Key]
        public int TourId { get; set; }

        [Required(ErrorMessage = "El nombre del tour es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un país")]
        public int PaisId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un destino")]
        public int DestinoId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateOnly Fecha { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria")]
        public TimeOnly Hora { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
        public decimal Precio { get; set; }

        public decimal Itbis { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La duración debe ser al menos 1 hora")]
        public int DuracionTotalHoras { get; set; }

        public DateTime FechaHoraFin { get; set; }

        public string Estado { get; set; } = string.Empty;

        [ForeignKey("PaisId")]
        public virtual Pai? Pais { get; set; }

        [ForeignKey("DestinoId")]
        public virtual Destino? Destino { get; set; }
    }
}