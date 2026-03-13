using System.Text.Json.Serialization;

namespace PornhubApiWrapper.Models;

public sealed class VideoTag
{
    [JsonPropertyName("tag_name")]
    public string? Name { get; set; }

    [JsonPropertyName("tag_url")]
    public string? Url { get; set; }
}
