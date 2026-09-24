using System.Text.Json.Serialization;

namespace HeartJourneyWeb.Services.MilestoneProgress;

public class MilestoneProgressRecord
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("journey_slug")]
    public string JourneySlug { get; set; } = string.Empty;

    [JsonPropertyName("milestone_slug")]
    public string MilestoneSlug { get; set; } = string.Empty;

    [JsonPropertyName("intro_view_count")]
    public int IntroViewCount { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime StartedAt { get; set; }

    [JsonPropertyName("last_visited_at")]
    public DateTime LastVisitedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}