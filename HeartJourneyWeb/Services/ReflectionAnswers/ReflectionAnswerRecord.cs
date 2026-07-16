using System.Text.Json.Serialization;

namespace HeartJourneyWeb.Services.ReflectionAnswers;

public class ReflectionAnswerRecord
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("journey_slug")]
    public string JourneySlug { get; set; } = string.Empty;

    [JsonPropertyName("milestone_slug")]
    public string MilestoneSlug { get; set; } = string.Empty;

    [JsonPropertyName("dimension_slug")]
    public string DimensionSlug { get; set; } = string.Empty;

    [JsonPropertyName("reflection_prompt_id")]
    public string ReflectionPromptId { get; set; } = string.Empty;

    [JsonPropertyName("reflection_prompt_slug")]
    public string? ReflectionPromptSlug { get; set; }

    [JsonPropertyName("reflection_prompt_title")]
    public string? ReflectionPromptTitle { get; set; }

    [JsonPropertyName("selected_answer_label")]
    public string SelectedAnswerLabel { get; set; } = string.Empty;

    [JsonPropertyName("selected_answer_description")]
    public string? SelectedAnswerDescription { get; set; }

    [JsonPropertyName("selected_status")]
    public string SelectedStatus { get; set; } = string.Empty;

    [JsonPropertyName("selected_concern_level")]
    public string SelectedConcernLevel { get; set; } = string.Empty;

    [JsonPropertyName("guidance_message")]
    public string? GuidanceMessage { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("answered_at")]
    public DateTime AnsweredAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}