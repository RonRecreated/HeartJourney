namespace HeartJourneyWeb.Services.Profiles;

public interface IProfileService
{
    HeartJourneyProfile? CurrentProfile { get; }

    Task<HeartJourneyProfile?> GetProfileAsync(
        CancellationToken cancellationToken = default);

    Task<HeartJourneyProfile> EnsureProfileAsync(
        CancellationToken cancellationToken = default);

    Task<HeartJourneyProfile> UpdateProfileAsync(
        HeartJourneyProfile profile,
        CancellationToken cancellationToken = default);
}