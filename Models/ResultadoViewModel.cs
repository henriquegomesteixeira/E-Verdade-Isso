namespace everdadeisso.Models
{
    public class ResultadoViewModel
    {
        public string Enviado { get; set; } = "";
        public string Classificacao { get; set; } = "";
        public string ExplicacaoHtml { get; set; } = "";
        public List<Referencia> Referencias { get; set; } = new();
    }
}
