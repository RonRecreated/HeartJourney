namespace HeartJourneyWeb.Features.Reflection;

public class InsightActionStepItem
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string ResourceLabel { get; set; } = string.Empty;

    public string ResourceUrl { get; set; } = string.Empty;

    public string PromptId { get; set; } = string.Empty;

    public string AnswerLabel { get; set; } = string.Empty;

    public bool IsExpanded { get; set; }

    public string Status { get; set; } = "not_started";
}