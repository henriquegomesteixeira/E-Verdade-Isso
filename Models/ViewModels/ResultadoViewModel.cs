namespace everdadeisso.Models.ViewModels
{
    public class ResultadoViewModel
    {
        public string Enviado { get; set; } = "";
        public string Classificacao { get; set; } = "";
        public string Explicacao { get; set; } = "";
        public List<Referencia> Referencias { get; set; } = new();
    }
}
