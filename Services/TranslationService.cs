using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class TranslationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TranslationService> _logger;

    public TranslationService(HttpClient httpClient, ILogger<TranslationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> TranslateAsync(string text, string targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(targetLanguage))
            return null;

        var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=en|{targetLanguage}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            var translatedText = doc.RootElement
                                    .GetProperty("responseData")
                                    .GetProperty("translatedText")
                                    .GetString();

            return translatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to translate text: {Text} to language: {Lang}", text, targetLanguage);
            return null;
        }
    }
}
