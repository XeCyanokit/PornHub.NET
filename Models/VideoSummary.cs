using System.Text.Json;
using System.Text.Json.Serialization;

namespace PornhubApiWrapper.Models;

public class VideoSummary
{
    [JsonPropertyName("video_id")]
    public string? VideoId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("default_thumb")]
    public string? DefaultThumbnailUrl { get; set; }

    [JsonPropertyName("thumb")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    [JsonPropertyName("views")]
    public long? Views { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("ratings")]
    public int? Ratings { get; set; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<VideoTag> Tags { get; set; } = [];

    [JsonPropertyName("pornstars")]
    public IReadOnlyList<Pornstar> Pornstars { get; set; } = [];

    [JsonPropertyName("categories")]
    public IReadOnlyList<Category> Categories { get; set; } = [];

    [JsonPropertyName("thumbs")]
    public IReadOnlyList<VideoThumbnail> Thumbs { get; set; } = [];

    [JsonPropertyName("embed_url")]
    public string? EmbedUrl { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
