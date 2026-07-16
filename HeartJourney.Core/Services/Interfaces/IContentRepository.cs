using HeartJourney.Core.Features.Home;
using HeartJourney.Core.Features.Journey;
using HeartJourney.Core.Features.Milestone;
using HeartJourney.Core.Features.Reflection;

namespace HeartJourney.Core.Services.Interfaces;

public interface IContentRepository
{
    Task<IReadOnlyList<JourneyCard>> GetJourneyCardsAsync(
        CancellationToken cancellationToken = default);

    Task<JourneyDetailView?> GetJourneyDetailBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<MilestoneDetailView?> GetMilestoneDetailBySlugsAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);

    Task<MilestoneIntroView?> GetMilestoneIntroBySlugsAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);

    Task<DimensionReflectionView?> GetDimensionReflectionBySlugsAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default);
}