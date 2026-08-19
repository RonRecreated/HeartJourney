namespace HeartJourney.Core.Services.Sanity.Queries;

public static class ReflectionQueries
{
    public static string GetDimensionReflectionBySlugs(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug)
    {
        var safeJourneySlug = EscapeGroqString(journeySlug);
        var safeMilestoneSlug = EscapeGroqString(milestoneSlug);
        var safeDimensionSlug = EscapeGroqString(dimensionSlug);

        return $$"""
            *[
                _type == "dimension"
                && slug.current == "{{safeDimensionSlug}}"
                && published == true
                && references(*[
                    _type == "milestone"
                    && slug.current == "{{safeMilestoneSlug}}"
                    && published == true
                    && journey->slug.current == "{{safeJourneySlug}}"
                    && journey->published == true
                ][0]._id)
            ][0]
            {
                "dimensionId": _id,
                "dimensionTitle": title,
                "dimensionSlug": slug.current,
                "dimensionSummary": summary,
                "dimensionIcon": icon,
                "healthyMarkers": coalesce(healthyMarkers, []),
                "warningSigns": coalesce(warningSigns, []),
                growthFocus,

                "journeySlug": *[
                    _type == "journey"
                    && slug.current == "{{safeJourneySlug}}"
                    && published == true
                ][0].slug.current,

                "journeyTitle": *[
                    _type == "journey"
                    && slug.current == "{{safeJourneySlug}}"
                    && published == true
                ][0].title,

                "milestoneSlug": *[
                    _type == "milestone"
                    && slug.current == "{{safeMilestoneSlug}}"
                    && published == true
                    && journey->slug.current == "{{safeJourneySlug}}"
                    && journey->published == true
                ][0].slug.current,

                "milestoneTitle": *[
                    _type == "milestone"
                    && slug.current == "{{safeMilestoneSlug}}"
                    && published == true
                    && journey->slug.current == "{{safeJourneySlug}}"
                    && journey->published == true
                ][0].title,

                "reflectionPrompt": *[
                    _type == "reflectionPrompt"
                    && published == true
                    && count(placements[
                        milestone->slug.current == "{{safeMilestoneSlug}}"
                        && milestone->published == true
                        && milestone->journey->slug.current == "{{safeJourneySlug}}"
                        && milestone->journey->published == true
                        && dimension->slug.current == "{{safeDimensionSlug}}"
                        && dimension->published == true
                    ]) > 0
                ]
                | order(placements[
                    milestone->slug.current == "{{safeMilestoneSlug}}"
                    && dimension->slug.current == "{{safeDimensionSlug}}"
                ][0].sortOrder asc)[0]
                {
                    "id": _id,
                    title,
                    "slug": slug.current,
                    question,
                    answerType,
                    "answerOptions": coalesce(answerOptions, [])
                        | order(sortOrder asc)
                        {
                            label,
                            description,
                            status,
                            concernLevel,
                            guidanceMessage,
                            sortOrder,

                            "nextPromptId": nextPrompt->_id,
                                "nextPromptSlug": nextPrompt->slug.current,

                                "endsPath": coalesce(endsPath, false),
                                "progressStatus": progressStatus,
                                "outcomeKey": outcomeKey,
                                "outcomeMessage": outcomeMessage,

                                "recommendedMilestoneSlug": recommendedMilestone->slug.current,
                                "recommendedMilestoneTitle": recommendedMilestone->title,

                                "actionSteps": coalesce(actionSteps, [])
                                    | order(sortOrder asc)
                                    {
                                        "key": _key,
                                        title,
                                        description,
                                        category,
                                        sortOrder,
                                        resourceLabel,
                                        resourceUrl
                                    }
                        },
                    allowNotes,
                    notesPrompt,
                    "sortOrder": placements[
                        milestone->slug.current == "{{safeMilestoneSlug}}"
                        && dimension->slug.current == "{{safeDimensionSlug}}"
                    ][0].sortOrder
                },
                    "reflectionPrompts": *[
                        _type == "reflectionPrompt"
                        && published == true
                        && count(placements[
                            milestone->slug.current == "{{safeMilestoneSlug}}"
                            && milestone->published == true
                            && milestone->journey->slug.current == "{{safeJourneySlug}}"
                            && milestone->journey->published == true
                            && dimension->slug.current == "{{safeDimensionSlug}}"
                            && dimension->published == true
                        ]) > 0
                    ]
                    | order(placements[
                        milestone->slug.current == "{{safeMilestoneSlug}}"
                        && dimension->slug.current == "{{safeDimensionSlug}}"
                    ][0].sortOrder asc)
                    {
                        "id": _id,
                        title,
                        "slug": slug.current,
                        question,
                        answerType,
                        "answerOptions": coalesce(answerOptions, [])
                            | order(sortOrder asc)
                            {
                                label,
                                description,
                                status,
                                concernLevel,
                                guidanceMessage,
                                sortOrder,

                                "nextPromptId": nextPrompt->_id,
                                "nextPromptSlug": nextPrompt->slug.current,

                                "endsPath": coalesce(endsPath, false),
                                "progressStatus": progressStatus,
                                "outcomeKey": outcomeKey,
                                "outcomeMessage": outcomeMessage,

                                "recommendedMilestoneSlug": recommendedMilestone->slug.current,
                                "recommendedMilestoneTitle": recommendedMilestone->title,

                                "actionSteps": coalesce(actionSteps, [])
                                    | order(sortOrder asc)
                                    {
                                        "key": _key,
                                        title,
                                        description,
                                        category,
                                        sortOrder,
                                        resourceLabel,
                                        resourceUrl
                                    }
                            },
                        allowNotes,
                        notesPrompt,
                        "sortOrder": placements[
                            milestone->slug.current == "{{safeMilestoneSlug}}"
                            && dimension->slug.current == "{{safeDimensionSlug}}"
                        ][0].sortOrder
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