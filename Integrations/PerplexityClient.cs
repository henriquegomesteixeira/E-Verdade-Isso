using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using everdadeisso.Interfaces;
using everdadeisso.Models.DTOs;

namespace everdadeisso.Integrations
{
    public class PerplexityClient : IPerplexityClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public PerplexityClient(HttpClient httpClient, IConfiguration config)
        {
            _http = httpClient;
            _apiKey = config["PERPLEXITY_API_KEY"];
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<RespostaDto> ObterRespostaDaPerplexityAsync(string texto)
        {
            var promptSystem = """
            Você é um verificador de fatos especializado que analisa informações com base em fontes confiáveis e verificáveis.

            ## Fontes Prioritárias
            Baseie-se em: fontes jornalísticas estabelecidas, documentos oficiais, estudos acadêmicos, dados governamentais e organizações reconhecidas. Evite blogs pessoais, redes sociais ou fontes não verificadas.

            ## Regras de Classificação

            **Para AFIRMAÇÕES** (declarações que podem ser julgadas como verdadeiras ou falsas):
            - "Informação verdadeira": Confirmada por múltiplas fontes confiáveis
            - "Informação falsa": Contradita por evidências claras e verificáveis  
            - "Informação duvidosa": Dados insuficientes, fontes conflitantes ou controvérsia legítima

            **Para PERGUNTAS** (quem é, o que é, como funciona, quando, onde, etc.):
            - "Informação contextual": Respostas explicativas baseadas em fatos

            Se o conteúdo enviado for um link de uma notícia:
            - Acesse o link, leia o conteúdo principal da página e entenda a informação que está sendo apresentada.
            - Verifique se essa informação é confirmada, desmentida ou debatida por outras fontes confiáveis.
            - Explique de forma clara se a notícia parece verdadeira, falsa, duvidosa ou apenas contextual.
            - Se o link estiver quebrado ou a página não for confiável, diga isso de forma educada.

            ## Instruções Gerais
            - Seja objetivo e direto
            - Não explique seu processo interno
            - Nunca invente classificações
            - Cite sempre as fontes utilizadas
            """;


            var promptUser = $"""
            Analise o seguinte conteúdo e forneça uma resposta estruturada no formato JSON especificado.

            **Conteúdo para análise:**
            {texto}

            **Instruções de formato:**
            - Classificação: use exatamente uma das opções válidas
            - Explicação: linguagem clara
            - Referências: use no mínimo 3 fontes, mais fontes são bem-vindas, formato exato conforme schema
            """;

            var schema = new
            {
                type = "object",
                properties = new
                {
                    classificacao = new
                    {
                        type = "string",
                        @enum = new[]
                        {
                            "Informação verdadeira",
                            "Informação falsa",
                            "Informação duvidosa",
                            "Informação contextual"
                        }
                    },
                    explicacao = new { type = "string" },
                    referencias = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                url = new { type = "string" },
                                titulo = new { type = "string" },
                                descricao = new { type = "string" }
                            },
                            required = new[] { "url", "titulo", "descricao" }
                        }
                    }
                },
                required = new[] { "classificacao", "explicacao", "referencias" }
            };

            var body = new
            {
                model = "sonar",
                messages = new[]
                {
                    new { role = "system", content = promptSystem },
                    new { role = "user", content = promptUser }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new { schema = schema }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("https://api.perplexity.ai/chat/completions", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro ao consultar a API da Perplexity.");

            var respostaJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respostaJson);

            var jsonString = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(jsonString))
                throw new Exception("Resposta vazia da Perplexity.");

            var respostaDto = JsonSerializer.Deserialize<RespostaDto>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (respostaDto == null)
                throw new Exception("Falha ao processar a resposta da IA.");

            return respostaDto;
        }
    }
}
