using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciaDeTours.Models
{
    public class Destino
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DestinoId { get; set; }

        [Required(ErrorMessage = "El nombre del destino es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty; // Inicializado para evitar advertencias

        [Required(ErrorMessage = "Debe seleccionar un país")]
        public int PaisId { get; set; }

        [Required(ErrorMessage = "La duración en días es obligatoria")]
        [Range(1, 365, ErrorMessage = "La duración debe ser entre 1 y 365 días")]
        public int DuracionDias { get; set; } = 1;

        [Required(ErrorMessage = "La duración en horas es obligatoria")]
        [Range(0, 23, ErrorMessage = "Las horas deben ser entre 0 y 23")]
        public int DuracionHoras { get; set; } = 0;

        [StringLength(50)]
        public string? Clima { get; set; } // Opcional, puede ser null

        [StringLength(50)]
        public string? TipoDestino { get; set; } // Opcional, puede ser null

        [StringLength(255)]
        public string? Actividades { get; set; } // Opcional, puede ser null

        [StringLength(100)]
        public string? AtractivoPrincipal { get; set; } // Opcional, puede ser null

        [StringLength(255)]
        public string? Descripcion { get; set; } // Opcional, puede ser null

        // Propiedad de navegación. Se marca como nullable (?) o se inicializa a null.
        // No necesitamos [ValidateNever] si gestionamos correctamente el ModelState.
        [ForeignKey("PaisId")]
        public virtual Pai? Pais { get; set; } // Marcar como nullable

        // Inicializar la colección para evitar null reference
        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}