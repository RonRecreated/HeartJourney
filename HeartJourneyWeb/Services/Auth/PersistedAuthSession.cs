namespace HeartJourneyWeb.Services.Auth;

public class PersistedAuthSession
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}