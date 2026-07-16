using System.Text.Json.Serialization;

namespace HeartJourney.Core.Features.Shared;

public class BibleVerseView
{
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}