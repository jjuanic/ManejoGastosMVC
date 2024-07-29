namespace PresuMVC.Models
{
    public class IndexViewModel
    {
        public DateTime Fecha { get; set; }
        public IEnumerable<Ingreso> Ingresos { get; set; }
        public IEnumerable<Egreso> Egresos { get; set; }
    }
}
