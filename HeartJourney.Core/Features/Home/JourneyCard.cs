namespace HeartJourney.Core.Features.Home;

public class JourneyCard
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Theme { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string HeroImageUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}