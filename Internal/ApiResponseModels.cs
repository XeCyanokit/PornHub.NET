using System.Text.Json;
using System.Text.Json.Serialization;
using PornhubApiWrapper.Models;

namespace PornhubApiWrapper.Internal;

internal sealed class VideoCollectionResponse
{
    [JsonPropertyName("videos")]
    public List<VideoSummary> Videos { get; set; } = [];

    [JsonPropertyName("total")]
    public long? Total { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

internal sealed class VideoDetailsResponse
{
    [JsonPropertyName("video")]
    public VideoDetails? Video { get; set; }
}

internal sealed class StarsResponse
{
    [JsonPropertyName("stars")]
    public List<StarEntry> Stars { get; set; } = [];
}

internal sealed class CategoriesResponse
{
    [JsonPropertyName("categories")]
    public List<Category> Categories { get; set; } = [];
}

internal sealed class TagsResponse
{
    [JsonPropertyName("tags")]
    public JsonElement Tags { get; set; }
}

internal sealed class ChannelsResponse
{
    [JsonPropertyName("channels")]
    public List<Channel> Channels { get; set; } = [];
}

internal sealed class VideoActiveResponse
{
    [JsonPropertyName("active")]
    public ActiveState? Active { get; set; }
}

internal sealed class ActiveState
{
    [JsonPropertyName("is_active")]
    public string? IsActive { get; set; }
}

internal sealed class StarEntry
{
    [JsonPropertyName("star")]
    public Pornstar? Star { get; set; }

    [JsonPropertyName("star_name")]
    public string? StarName { get; set; }

    [JsonPropertyName("star_url")]
    public string? StarUrl { get; set; }

    [JsonPropertyName("star_thumb")]
    public string? StarThumb { get; set; }

    [JsonPropertyName("pornstar_name")]
    public string? PornstarName { get; set; }

    [JsonPropertyName("pornstar_url")]
    public string? PornstarUrl { get; set; }

    [JsonPropertyName("pornstar_thumb")]
    public string? PornstarThumb { get; set; }
}
