using AutoMapper;
using FSPBook.OutboundService.Interfaces;
using FSPBook.OutboundService.Models;
using FSPBook.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FSPBook.OutboundService.Implementations;

public class NewsService : INewsService
{
    public const string TopNewsUrlKey = "OutboundServices:NewsApi:TopNewsUrl";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsService> _logger;
    private readonly IMapper _mapper;

    public NewsService(HttpClient httpClient, IConfiguration configuration, ILogger<NewsService> logger, IMapper mapper)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _mapper = mapper;
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

            var newsResponse = await response.Content.ReadFromJsonAsync<NewsApiResponse>();

            if (newsResponse?.Data is null)
            {
                _logger.LogWarning("No data found in news response");
                return new List<NewsItemViewModel>();
            }

            return _mapper.Map<List<NewsItemViewModel>>(newsResponse.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching technology news");
            return new List<NewsItemViewModel>();
        }
    }
}
