using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HeartJourneyWeb.Services.Auth;
using HeartJourneyWeb.Services.Supabase;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace HeartJourneyWeb.Services.Billing;

public sealed class BillingService(HttpClient http, IAuthService auth,
    IOptions<SupabaseOptions> options, IJSRuntime js) : IBillingService
{
    public Task<string> DetectProviderAsync() => js.InvokeAsync<string>("journeyBilling.detectProvider").AsTask();

    private const string RelationshipProductId =
    "relationship_journey_unlock";
    public async Task<PlayProduct> PrepareAsync(string journeySlug)
    {
        if (journeySlug != "relationship")
        {
            throw new InvalidOperationException(
                "Google Play billing is not configured for this journey.");
        }

        return await js.InvokeAsync<PlayProduct>(
            "journeyBilling.prepare",
            RelationshipProductId);
    }

    // The JS listener opens PaymentRequest synchronously in the native click event.
    public Task BindAsync<T>(ElementReference button, DotNetObjectReference<T> callback) where T : class =>
        js.InvokeVoidAsync("journeyBilling.bind", button, callback).AsTask();
    public Task UnbindAsync(ElementReference button) => js.InvokeVoidAsync("journeyBilling.unbind", button).AsTask();
    public Task<PlayPurchase[]> ListPurchasesAsync() => js.InvokeAsync<PlayPurchase[]>("journeyBilling.listPurchases").AsTask();
    public Task<PlayVerification> VerifyAsync(string journeySlug, string productId, string purchaseToken) =>
        SendAsync<PlayVerification>(new { action = "verify", journeySlug, productId, purchaseToken });

    private async Task<T> SendAsync<T>(object payload)
    {
        var token = await auth.GetValidAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Please sign in again before continuing.");
        using var request = new HttpRequestMessage(HttpMethod.Post,
            options.Value.Url.TrimEnd('/') + "/functions/v1/verify-google-play-purchase");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("apikey", options.Value.Key);
        request.Content = JsonContent.Create(payload);
        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            // Do not expose server internals or purchase tokens in the UI.
            throw new InvalidOperationException(response.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "This Google Play purchase is linked to another Journey Recreated account. Sign in with that account."
                : "Google Play could not confirm your purchase. Please try Restore Google Play purchase before paying again.");
        }
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new JsonException("Missing billing response.");
    }
}
