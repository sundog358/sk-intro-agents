﻿using Microsoft.SemanticKernel;
using SimpleFeedReader;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ServiceModel.Syndication;
using System.Web;

namespace SKDemo
{
    public class NewsArticle
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("publishDate")]
        public DateTime PublishDate { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
    }

    public class NewsPlugin
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public NewsPlugin()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
        }

        [KernelFunction("get_news")]
        [Description("Gets formatted news items for the specified category.")]
        [return: Description("A JSON string containing current news stories.")]
        public async Task<string> GetNews(
            Kernel kernel,
            [Description("The news category to fetch (e.g., Technology, Business, World)")] string category)
        {
            try
            {
                var reader = new FeedReader();
                var feedItems = await Task.Run(() => reader.RetrieveFeed($"https://rss.nytimes.com/services/xml/rss/nyt/{category}.xml")
                    .Take(10)
                    .Select(item => new NewsArticle
                    {
                        Title = HttpUtility.HtmlDecode(item.Title?.Trim() ?? ""),
                        Link = item.Uri?.ToString() ?? "",
                        PublishDate = item.PublishDate.UtcDateTime,
                        Summary = HttpUtility.HtmlDecode(item.Content?.Trim() ?? ""),
                        Category = category
                    })
                    .ToList());

                var response = new
                {
                    Status = "success",
                    Timestamp = DateTime.UtcNow,
                    Category = category,
                    Count = feedItems.Count,
                    Articles = feedItems
                };

                return JsonSerializer.Serialize(response, _jsonOptions);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Status = "error",
                    Timestamp = DateTime.UtcNow,
                    Message = ex.Message,
                    Category = category
                };

                return JsonSerializer.Serialize(errorResponse, _jsonOptions);
            }
        }
    }
}