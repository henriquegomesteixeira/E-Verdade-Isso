using System.Text.RegularExpressions;
using everdadeisso.Models;

namespace everdadeisso.Mappers
{
    public static class RespostaIaMapper
    {
        public static string LimparMarcadoresDeReferencia(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            return Regex.Replace(texto, @"\[\d+\]", "").Trim();
        }

        public static List<Referencia> FormatarReferenciasComDetalhes(List<Referencia> referencias)
        {
            var lista = new List<Referencia>();

            foreach (var referencia in referencias)
            {
                if (!Uri.TryCreate(referencia.Url, UriKind.Absolute, out var uri))
                    continue;

                lista.Add(new Referencia
                {
                    Url = referencia.Url,
                    Dominio = uri.Host,
                    NomeExibicao = uri.Host.Replace("www.", ""),
                    Titulo = referencia.Titulo,
                    Descricao = referencia.Descricao?.TrimEnd('.') ?? ""
                });
            }

            return lista;
        }
    }
}
