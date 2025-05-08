using System.Text;
using System.Text.Json;

public class OpenAIService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public OpenAIService(IConfiguration config)
    {
        _config = config;
        _http = new HttpClient();
    }

    public async Task<(List<string> Perguntas, string Dica)> GerarSugestoesEDicasAsync()
    {
        var apiKey = _config["OPENAI_API_KEY"];
        _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var body = new
        {
            model = "gpt-3.5-turbo-0125",
            messages = new[]
            {
                new {
                    role = "system",
                    content = "Você é um assistente objetivo que sugere perguntas populares e dá dicas para identificar fake news. Use linguagem direta e profissional, sem brincadeiras."
                },
                new {
                    role = "user",
                    content = """
                    Me envie 5 perguntas extremamente curtas (até 3 palavras), seguindo esta ordem de temas:
                    1. Esportes
                    2. Política
                    3. Economia
                    4. Saúde
                    5. Tecnologia

                    Depois, me envie 1 dica ou curiosidade educativa com até 200 caracteres. A dica deve ser clara, contextualizada (com dados ou exemplos reais), mas curta o suficiente para caber em um único parágrafo compacto. Não escreva título.

                    Liste tudo com 6 itens, um por linha, e apenas o conteúdo.
                    """
                }
            },
            temperature = 0.6
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
            return (new List<string> { "Erro ao consultar o ChatGPT" }, "");

        var resposta = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(resposta);
        var texto = doc.RootElement
                       .GetProperty("choices")[0]
                       .GetProperty("message")
                       .GetProperty("content")
                       .GetString() ?? "";

        var linhas = texto
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim().TrimStart('-', '*', '1', '2', '3', '4', '5', '6', '7', '8', '.'))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var perguntas = linhas.Take(5).ToList();
        var dica = (linhas.Skip(5).FirstOrDefault() ?? "")
            .Replace("Dica:", "", StringComparison.OrdinalIgnoreCase)
            .Trim();



        return (perguntas, dica);
    }
}
