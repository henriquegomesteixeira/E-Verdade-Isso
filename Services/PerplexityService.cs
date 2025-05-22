using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Markdig;
using everdadeisso.Models;
using HtmlAgilityPack;

namespace everdadeisso.Services
{
    public class PerplexityService
    {
        private readonly IConfiguration _config; // Acessa as configurações do appsettings.json
        private readonly HttpClient _http; // Permite fazer requisições HTTP

        public PerplexityService(IConfiguration config)
        {
            _config = config;
            _http = new HttpClient();
        }

        public async Task<(string classificacao, string explicacaoHtml, List<Referencia> referencias)> VerificarNoticiaAsync(string texto)
        {
            var apiKey = _config["PERPLEXITY_API_KEY"];
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Prepara o corpo (JSON) da requisição conforme o padrão exigido pela Perplexity
            var body = new
            {
                model = "sonar-reasoning",
                messages = new[]
                {
                    new {
                        role = "system",
                        content = "Você é um assistente que ajuda usuários a verificar se uma informação é verdadeira, falsa ou duvidosa."
                    },
                    new {
                        role = "user",
                        content = $"""
                        Responda exatamente neste formato:

                        Informação verdadeira (ou Informação falsa / Informação duvidosa)

                        **Explicação**  
                        (Aqui vai a explicação clara e objetiva)

                        **Referências**  
                        Liste os links usados no seguinte formato exato, tudo na mesma linha:

                        - https://exemplo.com/link1 :: Título da notícia :: O que esse link comprova  
                        - https://exemplo.com/link2 :: Título da notícia :: O que esse link comprova

                        Não adicione títulos adicionais, quebras de linha, nem asteriscos. Use apenas o traço (-), seguido do link, seguido de "::", seguido do título, seguido de "::", seguido da explicação. Tudo deve estar na mesma linha.

                        Conteúdo:  
                        \"\"\"  
                        {texto}
                        \"\"\"
                        """
                    }
                }
            };

            var json = JsonSerializer.Serialize(body); // Converte o objeto para JSON
            var content = new StringContent(json, Encoding.UTF8, "application/json");  // Cria um conteúdo HTTP com o JSON acima, dizendo que é do tipo application/json

            // Envia uma requisição POST para a API da Perplexity com o corpo formatado
            var response = await _http.PostAsync("https://api.perplexity.ai/chat/completions", content);

            if (!response.IsSuccessStatusCode)
                return ("Erro", "<p>Erro ao consultar a Perplexity.</p>", null);

            var resposta = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resposta);

            // Acessa o conteúdo gerado pela IA, que fica em: choices[0].message.content
            var mensagem = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            if (mensagem == null) return ("Erro", "<p>Nenhuma resposta encontrada.</p>", null);

            var (classificacao, markdownSemClassificacao) = ExtrairRespostaDireta(mensagem);

            var indexExplicacao = markdownSemClassificacao.IndexOf("**Explicação**", StringComparison.OrdinalIgnoreCase);
            var indexReferencias = markdownSemClassificacao.IndexOf("**Referências**", StringComparison.OrdinalIgnoreCase);

            string explicacaoMarkdown;
            string referenciasMarkdown;

            if (indexExplicacao != -1)
            {
                int inicioConteudoExplicacao = indexExplicacao + "**Explicação**".Length;

                if (indexReferencias != -1 && indexReferencias > inicioConteudoExplicacao)
                {
                    explicacaoMarkdown = markdownSemClassificacao.Substring(inicioConteudoExplicacao, indexReferencias - inicioConteudoExplicacao).Trim();
                    referenciasMarkdown = markdownSemClassificacao.Substring(indexReferencias).Trim();
                }
                else
                {
                    explicacaoMarkdown = markdownSemClassificacao.Substring(inicioConteudoExplicacao).Trim();
                    referenciasMarkdown = "";
                }
            }
            else
            {
                explicacaoMarkdown = markdownSemClassificacao;
                referenciasMarkdown = "";
            }

            explicacaoMarkdown = RemoverMarcacoesDeReferencia(explicacaoMarkdown);
            referenciasMarkdown = RemoverMarcacoesDeReferencia(referenciasMarkdown);

            var explicacaoHtml = ConverterMarkdownParaHtml(explicacaoMarkdown);
            var referenciasExtraidas = ExtrairReferenciasEstruturadas(referenciasMarkdown);

            Console.WriteLine($"Classificacao: {classificacao}");

            return (classificacao, explicacaoHtml, referenciasExtraidas);
        }

        private string ConverterMarkdownParaHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdown.ToHtml(markdown, pipeline);
            return html;
        }

        private (string classificacao, string restanteMensagem) ExtrairRespostaDireta(string mensagem)
        {
            var opcoes = new[]
            {
                "Informação verdadeira",
                "Informação falsa",
                "Informação duvidosa"
            };

            foreach (var opcao in opcoes)
            {
                var indice = mensagem.IndexOf(opcao, StringComparison.OrdinalIgnoreCase);
                if (indice != -1)
                {
                    var inicioDepoisClassificacao = indice + opcao.Length;
                    var restante = mensagem.Substring(inicioDepoisClassificacao).Trim();
                    return (opcao, restante);
                }
            }

            return ("Classificação não encontrada", mensagem);
        }


        private string RemoverMarcacoesDeReferencia(string markdown)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                markdown,
                @"\[\d+\]",
                ""
            );
        }

        private List<Referencia> ExtrairReferenciasEstruturadas(string markdown)
        {
            var lista = new List<Referencia>();

            var linhas = markdown
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.StartsWith("- ") && l.Contains("::"))
                .ToList();

            foreach (var linha in linhas)
            {
                var conteudo = linha.Substring(2);
                var partes = conteudo.Split("::", 3, StringSplitOptions.TrimEntries);

                if (partes.Length < 3 || !Uri.TryCreate(partes[0], UriKind.Absolute, out var uri))
                    continue;

                lista.Add(new Referencia
                {
                    Url = partes[0],
                    Dominio = uri.Host,
                    NomeExibicao = uri.Host.Replace("www.", ""),
                    Titulo = partes[1],
                    Descricao = partes[2].TrimEnd('.')
                });
            }

            return lista;
        }

    }
}
