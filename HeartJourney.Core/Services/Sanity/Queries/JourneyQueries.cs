namespace HeartJourney.Core.Services.Sanity.Queries;

public static class JourneyQueries
{
    public static string GetJourneyDetailBySlug(string slug)
    {
        var safeSlug = EscapeGroqString(slug);

        return $$"""
            *[_type == "journey" && slug.current == "{{safeSlug}}" && published == true][0]
            {
                "id": _id,
                title,
                "slug": slug.current,
                summary,
                purpose,
                theme,
                icon,
                "heroImageUrl": heroImage.asset->url,

                "milestones": *[
                    _type == "milestone"
                    && references(^._id)
                    && published == true
                ]
                | order(sortOrder asc)
                {
                    "id": _id,
                    title,
                    "slug": slug.current,
                    summary,
                    motto,
                    icon,
                    sortOrder
                }
            }
            """;
    }

    private static string EscapeGroqString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
}