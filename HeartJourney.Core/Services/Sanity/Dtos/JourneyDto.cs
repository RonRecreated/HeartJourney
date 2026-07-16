using System.Text.Json.Serialization;

namespace HeartJourney.Core.Services.Sanity;

public class JourneyDto
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool Published { get; set; }

    public SlugDto Slug { get; set; } = new();
}