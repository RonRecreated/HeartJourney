namespace HeartJourney.Core.Services.Sanity.Queries;

public static class MilestoneQueries
{
    public static string GetMilestoneDetailBySlugs(
        string journeySlug,
        string milestoneSlug)
    {
        var safeJourneySlug = EscapeGroqString(journeySlug);
        var safeMilestoneSlug = EscapeGroqString(milestoneSlug);

        return $$"""
            *[
                _type == "milestone"
                && slug.current == "{{safeMilestoneSlug}}"
                && published == true
                && journey->slug.current == "{{safeJourneySlug}}"
                && journey->published == true
            ][0]
            {
                "id": _id,
                title,
                "slug": slug.current,
                summary,
                purpose,
                motto,
                desiredOutcome,
                icon,
                sortOrder,
                "healthyCharacteristics": coalesce(healthyCharacteristics, []),
                "potentialPitfalls": coalesce(potentialPitfalls, []),

                "journeyTitle": journey->title,
                "journeySlug": journey->slug.current,

                "dimensions": coalesce(*[
                    _type == "dimension"
                    && published == true
                    && references(^._id)
                ]
                | order(sortOrder asc)
                {
                    "id": _id,
                    title,
                    "slug": slug.current,
                    summary,
                    icon,
                    sortOrder
                }, [])
            }
            """;
    }

    public static string GetMilestoneIntroBySlugs(
    string journeySlug,
    string milestoneSlug)
    {
        var safeJourneySlug = EscapeGroqString(journeySlug);
        var safeMilestoneSlug = EscapeGroqString(milestoneSlug);

        return $$"""
        *[
            _type == "milestone"
            && slug.current == "{{safeMilestoneSlug}}"
            && published == true
            && journey->slug.current == "{{safeJourneySlug}}"
            && journey->published == true
        ][0]
        {
            "id": _id,
            title,
            "slug": slug.current,
            summary,
            motto,

            "journeyTitle": journey->title,
            "journeySlug": journey->slug.current,

            introPositiveOutlook,
            introHelpsWith,

            "introBibleVerses": coalesce(introBibleVerses, []),

            introPrayerInvitation
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