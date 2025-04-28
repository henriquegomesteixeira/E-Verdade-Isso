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
                        role = "user",
                        content = $"""
                        Hoje é {DateTime.Now:dd/MM/yyyy}. Use esta data como referência ao verificar informações.
                        
                        Você é um sistema que ajuda usuários comuns da internet a descobrirem se uma informação é verdadeira, falsa ou duvidosa.

                        Atenção:  
                        - NÃO escreva introduções, pensamentos ou raciocínios (não use <think> nem explique seu raciocínio).  
                        - NÃO justifique antes de responder.  
                        - NÃO descreva o processo de análise.  
                        
                        1. Comece diretamente com uma destas palavras exatas:
                        - Informação verdadeira  
                        - Informação falsa  
                        - Informação duvidosa

                        2. Em seguida, escreva obrigatoriamente o título:
                        **Explicação**

                        3. Depois, escreva uma explicação clara e objetiva em português, como se estivesse explicando para alguém que não entende muito de tecnologia ou notícias.

                        4. Depois da explicação, obrigatoriamente escreva o título:
                        **Referências**

                        5. Em seguida, liste os links reais (URLs) usados como fonte.

                        Para cada link:
                        - Apresente o link completo e clicável (ex: https://...).
                        - Explique de forma clara e específica qual informação ele confirma ou desmente sobre o tema analisado.
                        - NÃO escreva descrições genéricas (ex: "artigo sobre eleições").
                        - A descrição deve deixar claro por que essa fonte comprova ou desmente a informação investigada.

                        Atenção:
                        - NÃO inicie os links diretamente sem antes escrever "**Referências**".  
                        - O título "**Referências**" deve aparecer separado da explicação.

                        IMPORTANTE:
                        - Utilize apenas fontes confiáveis e renomadas como:
                          - Sites de notícias de grande circulação (G1, Estadão, BBC, CNN Brasil, Agência Brasil, UOL, Folha de S.Paulo, etc).
                          - Fontes oficiais do governo (gov.br, tse.jus.br, agenciabrasil.ebc.com.br).
                          - Portais de checagem de fatos reconhecidos (Aos Fatos, Lupa, Estadão Verifica, AFP Checamos).
                        - NÃO use blogs, sites genéricos, tutoriais técnicos, fóruns ou páginas de dicas de internet.

                        Agora, verifique o seguinte conteúdo:  
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
