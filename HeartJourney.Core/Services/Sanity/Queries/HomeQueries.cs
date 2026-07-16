namespace HeartJourney.Core.Services.Sanity.Queries;

public static class HomeQueries
{
    public const string GetJourneyCards = """
        *[_type == "journey" && published == true]
        | order(sortOrder asc)
        {
            "id": _id,
            title,
            "slug": slug.current,
            summary,
            theme,
            icon,
            "heroImageUrl": heroImage.asset->url,
            sortOrder
        }
        """;
}