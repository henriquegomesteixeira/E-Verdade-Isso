namespace everdadeisso.Models
{
    public class Referencia
    {
        public string Url { get; set; }
        public string Dominio { get; set; }
        public string NomeExibicao { get; set; }
        public string Descricao { get; set; }
        public string Titulo { get; set; }

        public string FaviconUrl => $"https://www.google.com/s2/favicons?sz=64&domain={Dominio}";
    }
}
