using System.Text.Json.Serialization;

namespace PornhubApiWrapper.Models;

public sealed class VideoThumbnail
{
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("width")]
    public string? Width { get; set; }

    [JsonPropertyName("height")]
    public string? Height { get; set; }

    [JsonPropertyName("src")]
    public string? Source { get; set; }
}
