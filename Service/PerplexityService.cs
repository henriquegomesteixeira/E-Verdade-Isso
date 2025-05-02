using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Markdig;

namespace everdadeisso.Service
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

        public async Task<(string classificacao, string explicacaoHtml, string referenciasHtml)> VerificarNoticiaAsync(string texto)
        {
            var apiKey = _config["Perplexity:ApiKey"];
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
                        (Liste os links usados e uma frase curta explicando o que cada link comprova)
                        Cada item deve conter:  
                        - O link real e completo (ex: https://...)  
                        - Uma frase objetiva dizendo o que ele comprova

                        Não inclua apenas nomes de sites ou resumos sem o link. Cada item deve ter link + explicação.

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
                return ("Erro", "<p>Erro ao consultar a Perplexity.</p>", "");

            var resposta = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resposta);

            // Acessa o conteúdo gerado pela IA, que fica em: choices[0].message.content
            var mensagem = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            if (mensagem == null) return ("Erro", "<p>Nenhuma resposta encontrada.</p>", "");

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
            var referenciasHtml = ConverterMarkdownParaHtml(referenciasMarkdown);

            return (classificacao, explicacaoHtml, referenciasHtml);
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
    }
}
