namespace HeartJourney.Core.Features.Milestone;

public class MilestoneDetailView
{
    public string Id { get; set; } = string.Empty;

    public string JourneySlug { get; set; } = string.Empty;

    public string JourneyTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public string Motto { get; set; } = string.Empty;

    public string DesiredOutcome { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public List<string> HealthyCharacteristics { get; set; } = new();

    public List<string> PotentialPitfalls { get; set; } = new();

    public List<DimensionCard> Dimensions { get; set; } = new();
}