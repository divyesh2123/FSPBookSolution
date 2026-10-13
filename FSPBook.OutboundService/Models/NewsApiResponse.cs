using System.Text.Json.Serialization;

namespace FSPBook.OutboundService.Models;

/// <summary>
/// Response contract for https://api.thenewsapi.com/v1/news/top.
/// Only the fields we use are mapped; anything else in the payload is ignored.
/// </summary>
public class NewsApiResponse
{
    [JsonPropertyName("data")]
    public List<NewsApiArticle>? Data { get; set; }
}

public class NewsApiArticle
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
