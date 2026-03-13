using System.Text.Json.Serialization;

namespace PornhubApiWrapper.Models;

public sealed class Channel
{
    [JsonPropertyName("channel_name")]
    public string? Name { get; set; }

    [JsonPropertyName("channel_url")]
    public string? Url { get; set; }

    [JsonPropertyName("channel_thumb")]
    public string? ThumbnailUrl { get; set; }
}
