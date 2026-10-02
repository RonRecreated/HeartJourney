namespace HeartJourneyWeb.Services.JourneyAccess;

public interface IJourneyAccessService
{
    Task<bool> HasAccessAsync(
        string journeySlug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JourneyAccessRecord>> GetUserAccessAsync(
        CancellationToken cancellationToken = default);
}