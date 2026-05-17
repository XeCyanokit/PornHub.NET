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

    /// <summary>Rank on the public channels listing (scraped HTML only).</summary>
    [JsonIgnore]
    public int? Rank { get; set; }

    /// <summary>Subscriber / video count line from listing (scraped HTML only).</summary>
    [JsonIgnore]
    public string? StatsLine { get; set; }
}
