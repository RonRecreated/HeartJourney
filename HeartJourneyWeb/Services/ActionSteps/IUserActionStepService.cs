namespace HeartJourneyWeb.Services.ActionSteps;

public interface IUserActionStepService
{
    Task<IReadOnlyList<UserActionStepRecord>> GetActionStepsForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default);

    Task<UserActionStepRecord> SaveActionStepStatusAsync(
        SaveUserActionStepRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteActionStepsForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default);
}