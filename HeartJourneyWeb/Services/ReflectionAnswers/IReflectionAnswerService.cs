namespace HeartJourneyWeb.Services.ReflectionAnswers;

public interface IReflectionAnswerService
{
    Task<ReflectionAnswerRecord?> GetAnswerForPromptAsync(
        string reflectionPromptId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReflectionAnswerRecord>> GetAnswersForPromptsAsync(
        IReadOnlyList<string> reflectionPromptIds,
        CancellationToken cancellationToken = default);

    Task<ReflectionAnswerRecord> SaveAnswerAsync(
        SaveReflectionAnswerRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReflectionAnswerRecord>> GetAnswersForMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default);

    Task DeleteAnswersForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReflectionAnswerRecord>> GetAnswersForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default);
}