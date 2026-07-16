using System.Text.Json.Serialization;

namespace HeartJourneyWeb.Services.Profiles;

public class HeartJourneyProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("age_bracket")]
    public string? AgeBracket { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("religious_background")]
    public string? ReligiousBackground { get; set; }

    [JsonPropertyName("current_relationship_season")]
    public string? CurrentRelationshipSeason { get; set; }

    [JsonPropertyName("onboarding_completed")]
    public bool OnboardingCompleted { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}