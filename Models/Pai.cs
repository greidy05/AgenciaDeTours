using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgenciaDeTours.Models
{
    [Table("Pais")]
    [Index(nameof(CodigoIso), IsUnique = true, Name = "IX_Pais_CodigoIso_Unique")]
    public partial class Pai
    {
        [Key]
        [Column("PaisId")]
        public int PaisId { get; set; }

        [Required(ErrorMessage = "El nombre del país es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = null!;

        [StringLength(3)] // Eliminamos las validaciones que ya no son necesarias
        [Display(Name = "Código ISO (Auto-generado)")]
        public string? CodigoIso { get; set; } // Cambiado a nullable

        // Resto de propiedades se mantienen igual...
        [Display(Name = "Idioma Oficial")]
        [StringLength(50, ErrorMessage = "El idioma no puede exceder 50 caracteres")]
        public string? IdiomaOficial { get; set; }

        [StringLength(30, ErrorMessage = "La moneda no puede exceder 30 caracteres")]
        public string? Moneda { get; set; }

        [StringLength(30, ErrorMessage = "El continente no puede exceder 30 caracteres")]
        public string? Continente { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        public virtual ICollection<Destino> Destinos { get; set; } = new List<Destino>();
        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}