using System.ComponentModel.DataAnnotations;

namespace PresuMVC.Models
{
    public class Ingreso : IValidatableObject
    {
        public int IdIngreso { get; set; }
        public string Nombre { get; set; }
        public float Valor { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaFin { get; set; }
        public int IdTipoIngreso { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin.HasValue && FechaFin.Value < FechaRegistro)
            {
                yield return new ValidationResult("La Fecha Fin no puede ser menor que la Fecha de Registro", new[] { "FechaFin" });
            }
        }
    }
}
