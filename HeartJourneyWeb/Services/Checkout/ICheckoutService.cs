namespace HeartJourneyWeb.Services.Checkout;

public interface ICheckoutService
{
    Task<string> CreateCheckoutAsync(
        string journeySlug,
        CancellationToken cancellationToken = default,
        string? returnPath = null);
}
