namespace HeartJourneyWeb.Helpers;

// PAYWALL PREVIEW: shared, explicit navigation rules keep protected pages protected.
public static class JourneyPreviewNavigation
{
    public static bool IsPreviewPage(Type pageType) =>
        pageType == typeof(Features.Milestone.MilestoneIntroPage) ||
        pageType == typeof(Features.Milestone.MilestoneResumePage);

    // Only local app paths may be carried through authentication or checkout.
    public static string SafeReturnUrl(string? path, string fallback = "/journeys")
    {
        if (string.IsNullOrWhiteSpace(path)) return fallback;
        var decoded = path;
        // Validate encoded paths too, so encoded slashes/backslashes cannot become external redirects.
        for (var attempt = 0; attempt < 4; attempt++)
        {
            if (!decoded.StartsWith('/') || decoded.StartsWith("//") ||
                decoded.Contains('\\') || decoded.Any(char.IsControl)) return fallback;
            var next = Uri.UnescapeDataString(decoded);
            if (next == decoded) return path;
            decoded = next;
        }
        return fallback;
    }

    public static string IntroUrl(string journeySlug, string milestoneSlug, bool fourthCard = false) =>
        $"/journeys/{Uri.EscapeDataString(journeySlug)}/milestones/{Uri.EscapeDataString(milestoneSlug)}/intro" +
        (fourthCard ? "?step=4" : string.Empty);
}
