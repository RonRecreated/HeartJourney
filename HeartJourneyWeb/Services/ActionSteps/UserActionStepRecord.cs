using System.Text.Json.Serialization;

namespace HeartJourneyWeb.Services.ActionSteps;

public class UserActionStepRecord
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

    [JsonPropertyName("action_step_key")]
    public string ActionStepKey { get; set; } = string.Empty;

    [JsonPropertyName("action_step_title")]
    public string ActionStepTitle { get; set; } = string.Empty;

    [JsonPropertyName("action_step_category")]
    public string? ActionStepCategory { get; set; }

    [JsonPropertyName("reflection_prompt_id")]
    public string? ReflectionPromptId { get; set; }

    [JsonPropertyName("selected_answer_label")]
    public string? SelectedAnswerLabel { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "not_started";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }
}