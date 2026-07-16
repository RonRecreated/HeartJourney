namespace HeartJourney.Core.Features.Reflection;

public class DimensionReflectionView
{
    public string JourneySlug { get; set; } = string.Empty;

    public string JourneyTitle { get; set; } = string.Empty;

    public string MilestoneSlug { get; set; } = string.Empty;

    public string MilestoneTitle { get; set; } = string.Empty;

    public string DimensionId { get; set; } = string.Empty;

    public string DimensionTitle { get; set; } = string.Empty;

    public string DimensionSlug { get; set; } = string.Empty;

    public string DimensionSummary { get; set; } = string.Empty;

    public string DimensionIcon { get; set; } = string.Empty;

    public List<string> HealthyMarkers { get; set; } = new();

    public List<string> WarningSigns { get; set; } = new();

    public string GrowthFocus { get; set; } = string.Empty;

    //Keep temporary for now, until we have a better way to handle the reflection prompts
    public ReflectionPromptView? ReflectionPrompt { get; set; }

    //New sequencing list of reflection prompts, to be used
    public List<ReflectionPromptView> ReflectionPrompts { get; set; } = new();
}