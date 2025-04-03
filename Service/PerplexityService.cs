using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

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

        public async Task<string> VerificarNoticiaAsync(string texto)
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

                        Analise o conteúdo abaixo com base em fontes confiáveis da internet e responda no seguinte formato:

                        1. **Classificação** (escreva exatamente uma destas palavras no início da resposta):  
                           - Informação verdadeira  
                           - Informação falsa  
                           - Informação duvidosa

                        2. **Explicação clara e objetiva**, em português, como se estivesse explicando para alguém que não entende muito de tecnologia ou notícias.

                        3. 3. **Referências**: no final, traga os **links reais (URLs)** usados como fonte. Para cada link, inclua uma frase curta explicando o que ele comprova. Os links devem estar completos e clicáveis (ex: https://...).

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
            {
                return "Erro ao consultar a Perplexity.";
            }

            var resposta = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resposta);

            // Acessa o conteúdo gerado pela IA, que fica em: choices[0].message.content
            var mensagem = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return mensagem ?? "Nenhuma resposta encontrada.";
        }
    }
}
