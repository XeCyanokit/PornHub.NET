using System.Text.Json.Serialization;

namespace PornhubApiWrapper.Models;

public sealed class Pornstar
{
    [JsonPropertyName("pornstar_name")]
    public string? Name { get; set; }

    [JsonPropertyName("pornstar_url")]
    public string? Url { get; set; }

    [JsonPropertyName("pornstar_thumb")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("star_name")]
    public string? AlternateName { get; set; }

    [JsonPropertyName("star_url")]
    public string? AlternateUrl { get; set; }

    [JsonPropertyName("star_thumb")]
    public string? AlternateThumbnailUrl { get; set; }
}
