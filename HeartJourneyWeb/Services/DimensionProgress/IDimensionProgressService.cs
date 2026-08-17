namespace HeartJourneyWeb.Services.DimensionProgress;

public interface IDimensionProgressService
{
    Task<DimensionProgressRecord?> GetProgressAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DimensionProgressRecord>> GetProgressForMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);

    Task<DimensionProgressRecord> SaveProgressAsync(
        SaveDimensionProgressRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteProgressForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default);
}