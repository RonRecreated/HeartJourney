namespace HeartJourneyWeb.Services.ReflectionAnswers;

public class SaveReflectionAnswerRequest
{
    public string JourneySlug { get; set; } = string.Empty;

    public string MilestoneSlug { get; set; } = string.Empty;

    public string DimensionSlug { get; set; } = string.Empty;

    public string ReflectionPromptId { get; set; } = string.Empty;

    public string? ReflectionPromptSlug { get; set; }

    public string? ReflectionPromptTitle { get; set; }

    public string SelectedAnswerLabel { get; set; } = string.Empty;

    public string? SelectedAnswerDescription { get; set; }

    public string SelectedStatus { get; set; } = string.Empty;

    public string SelectedConcernLevel { get; set; } = string.Empty;

    public string? GuidanceMessage { get; set; }

    public string? Notes { get; set; }
}