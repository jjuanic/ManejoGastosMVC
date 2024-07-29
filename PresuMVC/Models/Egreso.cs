namespace PresuMVC.Models
{
    public class Egreso
    {
        public int IdEgreso { get; set; }
        public string Nombre { get; set; }
        public float Valor {  get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? CuotaNro {  get; set; }
        public int? CantCuotas {  get; set; }
        public DateTime? FechaCuota { get; set; }
        public float? ValorTotal { get; set; }
        public int IdTipoEgreso { get; set; }
        public int? IdEgresoOriginal { get; set; }

    }
}
