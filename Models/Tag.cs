using System.Text.Json.Serialization;

namespace PornhubApiWrapper.Models;

public sealed class Tag
{
    [JsonPropertyName("tag_name")]
    public string? Name { get; set; }

    [JsonPropertyName("tag_url")]
    public string? Url { get; set; }

    [JsonPropertyName("tag_thumb")]
    public string? ThumbnailUrl { get; set; }
}
