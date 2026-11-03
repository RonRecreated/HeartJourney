using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HeartJourneyWeb.Services.Billing;

public interface IBillingService
{
    Task<string> DetectProviderAsync();
    Task<PlayProduct> PrepareAsync(string journeySlug);
    Task BindAsync<T>(ElementReference button, DotNetObjectReference<T> callback) where T : class;
    Task UnbindAsync(ElementReference button);
    Task<PlayVerification> VerifyAsync(string journeySlug, string productId, string purchaseToken);
    Task<PlayPurchase[]> ListPurchasesAsync();
}

public sealed record PlayProduct(string ProductId, string Price);
public sealed record PlayPurchase(string ItemId, string PurchaseToken);
public sealed record PlayVerification(string Status);
