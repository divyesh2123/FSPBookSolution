using FSPBook.OutboundService.Interfaces;
using FSPBook.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FSPBook.OutboundService.Implementations;

public class NewsService : INewsService
{
    public const string TopNewsUrlKey = "OutboundServices:NewsApi:TopNewsUrl";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsService> _logger;

    public NewsService(HttpClient httpClient, IConfiguration configuration, ILogger<NewsService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<NewsItemViewModel>> GetTechnologyNewsAsync(int limit = 5)
    {
        try
        {
            var apiKey = _configuration["NewsApiKey"];
            var filterCategory = _configuration["filterCategory"];
            var topNewsUrl = _configuration[TopNewsUrlKey];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("NewsApiKey is not configured");
                return new List<NewsItemViewModel>();
            }

            if (string.IsNullOrWhiteSpace(topNewsUrl))
            {
                _logger.LogWarning("{Key} is not configured", TopNewsUrlKey);
                return new List<NewsItemViewModel>();
            }

            var url = $"{topNewsUrl}?api_token={apiKey}&categories={filterCategory}&language=en&limit={limit}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch news: {response.StatusCode}");
                return new List<NewsItemViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var data))
            {
                _logger.LogWarning("No data found in news response");
                return new List<NewsItemViewModel>();
            }

            var news = new List<NewsItemViewModel>();
            foreach (var element in data.EnumerateArray())
            {
                var title = element.GetProperty("title").GetString() ?? string.Empty;
                var url_link = element.TryGetProperty("url", out var u) ? u.GetString() : null;
                var source = element.TryGetProperty("source", out var s) ? s.GetString() : null;

                news.Add(new NewsItemViewModel
                {
                    Title = title,
                    Url = url_link,
                    Source = source
                });
            }

            return news;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching technology news");
            return new List<NewsItemViewModel>();
        }
    }
}
