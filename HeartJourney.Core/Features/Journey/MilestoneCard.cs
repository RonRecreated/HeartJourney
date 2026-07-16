namespace HeartJourney.Core.Features.Journey;

public class MilestoneCard
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Motto { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}