using System.Text.Json.Serialization;

namespace HeartJourneyWeb.Services.DimensionProgress;

public class DimensionProgressRecord
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

    [JsonPropertyName("status")]
    public string Status { get; set; } = "in_progress";

    [JsonPropertyName("current_prompt_id")]
    public string? CurrentPromptId { get; set; }

    [JsonPropertyName("current_prompt_slug")]
    public string? CurrentPromptSlug { get; set; }

    [JsonPropertyName("outcome_key")]
    public string? OutcomeKey { get; set; }

    [JsonPropertyName("outcome_message")]
    public string? OutcomeMessage { get; set; }

    [JsonPropertyName("recommended_milestone_slug")]
    public string? RecommendedMilestoneSlug { get; set; }

    [JsonPropertyName("recommended_milestone_title")]
    public string? RecommendedMilestoneTitle { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("visible_question_number")]
    public int VisibleQuestionNumber { get; set; } = 1;

    [JsonPropertyName("prompt_history_json")]
    public string? PromptHistoryJson { get; set; }
}