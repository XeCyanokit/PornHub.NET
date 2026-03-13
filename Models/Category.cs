using System.Text.Json.Serialization;

namespace PornhubApiWrapper.Models;

public sealed class Category
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("category")]
    public string? Name { get; set; }

    [JsonPropertyName("category_url")]
    public string? Url { get; set; }

    [JsonPropertyName("category_thumb")]
    public string? ThumbnailUrl { get; set; }
}
