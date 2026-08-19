using System.Text.Json.Serialization;

namespace HeartJourney.Core.Features.Reflection;

public class ActionStepView
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    [JsonPropertyName("resourceLabel")]
    public string ResourceLabel { get; set; } = string.Empty;

    [JsonPropertyName("resourceUrl")]
    public string ResourceUrl { get; set; } = string.Empty;
}