namespace HeartJourneyWeb.Services.MilestoneReset;

public interface IMilestoneResetService
{
    Task ResetMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);
}