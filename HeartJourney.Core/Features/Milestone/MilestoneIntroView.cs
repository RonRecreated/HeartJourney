using System.Text.Json;
using System.Text.Json.Serialization;
using HeartJourney.Core.Features.Shared;

namespace HeartJourney.Core.Features.Milestone;

public class MilestoneIntroView
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("journeySlug")]
    public string JourneySlug { get; set; } = string.Empty;

    [JsonPropertyName("journeyTitle")]
    public string JourneyTitle { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("motto")]
    public string Motto { get; set; } = string.Empty;

    [JsonPropertyName("introPositiveOutlook")]
    public string IntroPositiveOutlook { get; set; } = string.Empty;

    [JsonPropertyName("introHelpsWith")]
    public string IntroHelpsWith { get; set; } = string.Empty;

    [JsonPropertyName("introBibleVerses")]
    public List<BibleVerseView> IntroBibleVerses { get; set; } = new();

    [JsonPropertyName("introPrayerInvitation")]
    public string IntroPrayerInvitation { get; set; } = string.Empty;
}