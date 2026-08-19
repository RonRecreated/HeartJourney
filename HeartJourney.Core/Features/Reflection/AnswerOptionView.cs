namespace HeartJourney.Core.Features.Reflection;

public class AnswerOptionView
{
    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ConcernLevel { get; set; } = string.Empty;

    public string GuidanceMessage { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string? NextPromptId { get; set; }

    public string? NextPromptSlug { get; set; }

    public bool EndsPath { get; set; }

    public string? ProgressStatus { get; set; }

    public string? OutcomeKey { get; set; }

    public string? OutcomeMessage { get; set; }

    public string? RecommendedMilestoneSlug { get; set; }

    public string? RecommendedMilestoneTitle { get; set; }

    public List<ActionStepView> ActionSteps { get; set; } = new();
}