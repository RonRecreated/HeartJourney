using HeartJourney.Core.Features.Home;
using HeartJourney.Core.Features.Journey;
using HeartJourney.Core.Features.Milestone;
using HeartJourney.Core.Features.Reflection;
using HeartJourney.Core.Services.Interfaces;
using HeartJourney.Core.Services.Sanity.Queries;

namespace HeartJourney.Core.Services.Sanity;

public class ContentRepository : IContentRepository
{
    private readonly SanityClient _sanityClient;

    public ContentRepository(SanityClient sanityClient)
    {
        _sanityClient = sanityClient;
    }

    public async Task<IReadOnlyList<JourneyCard>> GetJourneyCardsAsync(
        CancellationToken cancellationToken = default)
    {
        var journeys = await _sanityClient.QueryAsync<List<JourneyCard>>(
            HomeQueries.GetJourneyCards,
            cancellationToken);

        return journeys;
    }

    public async Task<JourneyDetailView?> GetJourneyDetailBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _sanityClient.QueryAsync<JourneyDetailView?>(
            JourneyQueries.GetJourneyDetailBySlug(slug),
            cancellationToken);
    }

    public async Task<MilestoneDetailView?> GetMilestoneDetailBySlugsAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        return await _sanityClient.QueryAsync<MilestoneDetailView?>(
            MilestoneQueries.GetMilestoneDetailBySlugs(journeySlug, milestoneSlug),
            cancellationToken);
    }

    public async Task<MilestoneIntroView?> GetMilestoneIntroBySlugsAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        return await _sanityClient.QueryAsync<MilestoneIntroView?>(
            MilestoneQueries.GetMilestoneIntroBySlugs(journeySlug, milestoneSlug),
            cancellationToken);
    }

    public async Task<DimensionReflectionView?> GetDimensionReflectionBySlugsAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default)
    {
        return await _sanityClient.QueryAsync<DimensionReflectionView?>(
            ReflectionQueries.GetDimensionReflectionBySlugs(
                journeySlug,
                milestoneSlug,
                dimensionSlug),
            cancellationToken);
    }
}