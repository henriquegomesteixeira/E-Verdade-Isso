using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenAIClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public OpenAIClient(IConfiguration config)
    {
        _apiKey = config["OPENAI_API_KEY"];
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> EnviarPromptAsync(object prompt)
    {
        var content = new StringContent(JsonSerializer.Serialize(prompt), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Erro ao consultar o ChatGPT");

        return await response.Content.ReadAsStringAsync();
    }
}
