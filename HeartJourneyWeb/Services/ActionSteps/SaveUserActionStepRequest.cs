namespace HeartJourneyWeb.Services.ActionSteps;

public class SaveUserActionStepRequest
{
    public string JourneySlug { get; set; } = string.Empty;

    public string MilestoneSlug { get; set; } = string.Empty;

    public string DimensionSlug { get; set; } = string.Empty;

    public string ActionStepKey { get; set; } = string.Empty;

    public string ActionStepTitle { get; set; } = string.Empty;

    public string? ActionStepCategory { get; set; }

    public string? ReflectionPromptId { get; set; }

    public string? SelectedAnswerLabel { get; set; }

    public string Status { get; set; } = "not_started";
}