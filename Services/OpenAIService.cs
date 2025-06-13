using System.Text.Json;

public class OpenAIService
{
    private readonly OpenAIClient _client;

    public OpenAIService(OpenAIClient client)
    {
        _client = client;
    }

    public async Task<(List<string> Perguntas, string Dica)> GerarSugestoesEDicasAsync()
    {
        var prompt = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new {
                    role = "system",
                    content = """
                    Você é um especialista em fact-checking que identifica temas atuais propensos a fake news. 
                    Crie perguntas curtas (máximo 4 palavras) sobre assuntos que frequentemente geram desinformação.
                    """
                },
                new {
                    role = "user",
                    content = """
                    Me envie 5 perguntas extremamente curtas (até 3 palavras), seguindo esta ordem de temas:
                    Esportes
                    Política
                    Economia
                    Saúde
                    Tecnologia

                    Depois, me envie 1 dica ou curiosidade educativa com até 200 caracteres. A dica deve ser clara, contextualizada (com dados ou exemplos reais), mas curta o suficiente para caber em um único parágrafo compacto. Não escreva título.

                    Liste tudo com 6 itens, um por linha, e apenas o conteúdo.
                    """
                }
            },
            temperature = 0.6
        };

        var respostaJson = await _client.EnviarPromptAsync(prompt);

        using var doc = JsonDocument.Parse(respostaJson);
        var texto = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";

        var linhas = texto
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim().TrimStart('-', '*', '.'))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var perguntas = linhas.Take(5).ToList();
        var dica = (linhas.Skip(5).FirstOrDefault() ?? "")
            .Replace("Dica:", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return (perguntas, dica);
    }
}
