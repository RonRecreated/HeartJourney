namespace HeartJourneyWeb.Helpers;

public static class IconHelper
{
    public static string GetIconClass(string? iconName)
    {
        return iconName?.Trim().ToLowerInvariant() switch
        {
            // General
            "home" => "fa-solid fa-house",
            "settings" => "fa-solid fa-gear",
            "user" => "fa-solid fa-user",
            "users" => "fa-solid fa-users",
            "heart" => "fa-solid fa-heart",
            "comments" => "fa-solid fa-comments",
            "book" => "fa-solid fa-book",
            "bible" => "fa-solid fa-book-bible",
            "pray" => "fa-solid fa-hands-praying",
            "calendar" => "fa-solid fa-calendar",
            "check" => "fa-solid fa-check",
            "warning" => "fa-solid fa-triangle-exclamation",
            "info" => "fa-solid fa-circle-info",
            "restart" => "fa-solid fa-rotate-left",
            "arrow-right" => "fa-solid fa-arrow-right",

            // Journey / growth
            "journey" => "fa-solid fa-route",
            "path" => "fa-solid fa-route",
            "compass" => "fa-solid fa-compass",
            "seedling" => "fa-solid fa-seedling",
            "growth" => "fa-solid fa-seedling",
            "lightbulb" => "fa-solid fa-lightbulb",

            // Relationship milestones
            "single" => "fa-solid fa-user",
            "talking" => "fa-solid fa-comment",
            "courting" => "fa-solid fa-heart",
            "engaged" => "fa-solid fa-award",
            "married" => "fa-solid fa-people-roof",
            "divorced" => "fa-solid fa-heart-crack",

            // Finance
            "finance" => "fa-solid fa-wallet",
            "money" => "fa-solid fa-coins",
            "budget" => "fa-solid fa-chart-pie",
            "wallet" => "fa-solid fa-wallet",

            _ => "fa-solid fa-circle"
        };
    }
}