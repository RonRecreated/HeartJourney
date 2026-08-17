namespace HeartJourneyWeb.Services.DimensionProgress;

public class SaveDimensionProgressRequest
{
    public string JourneySlug { get; set; } = string.Empty;

    public string MilestoneSlug { get; set; } = string.Empty;

    public string DimensionSlug { get; set; } = string.Empty;

    public string Status { get; set; } = "in_progress";

    public string? CurrentPromptId { get; set; }

    public string? CurrentPromptSlug { get; set; }

    public int VisibleQuestionNumber { get; set; } = 1;

    public string? PromptHistoryJson { get; set; }

    public string? OutcomeKey { get; set; }

    public string? OutcomeMessage { get; set; }

    public string? RecommendedMilestoneSlug { get; set; }

    public string? RecommendedMilestoneTitle { get; set; }

    public DateTime? CompletedAt { get; set; }

}