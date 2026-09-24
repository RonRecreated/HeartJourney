namespace HeartJourneyWeb.Services.MilestoneProgress;

public interface IMilestoneProgressService
{
    Task<MilestoneProgressRecord?> GetProgressAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MilestoneProgressRecord>> GetProgressForJourneyAsync(
        string journeySlug,
        CancellationToken cancellationToken = default);

    Task<MilestoneProgressRecord> StartMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);

    Task<MilestoneProgressRecord> CompleteIntroVisitAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);
}