using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HeartJourneyWeb.Services.Auth;
using Microsoft.Extensions.Options;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.Checkout;

public class CheckoutService : ICheckoutService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly AppSupabaseOptions _options;
    private readonly Microsoft.AspNetCore.Components.NavigationManager _navigation;

    public CheckoutService(
        HttpClient httpClient,
        IAuthService authService,
        IOptions<AppSupabaseOptions> options,
        Microsoft.AspNetCore.Components.NavigationManager navigation)
    {
        _httpClient = httpClient;
        _authService = authService;
        _options = options.Value;
        _navigation = navigation;
    }

    public async Task<string> CreateCheckoutAsync(
        string journeySlug,
        CancellationToken cancellationToken = default,
        string? returnPath = null)
    {
        var accessToken =
            await _authService.GetValidAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "Your session has expired. Please sign in again.");
        }

        var url =
            $"{_options.Url.TrimEnd('/')}" +
            "/functions/v1/create-checkout";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Headers.TryAddWithoutValidation(
            "apikey",
            _options.Key);

        // PAYWALL PREVIEW: the function must validate these URLs against the app's allowed origin.
        // Existing functions may ignore them; browser recovery retains the selected milestone too.
        var safePath = HeartJourneyWeb.Helpers.JourneyPreviewNavigation.SafeReturnUrl(returnPath);
        var separator = safePath.Contains('?') ? "&" : "?";
        request.Content =
            JsonContent.Create(
                new
                {
                    journeySlug,
                    successUrl = _navigation.ToAbsoluteUri(safePath + separator + "checkout=success").ToString(),
                    cancelUrl = _navigation.ToAbsoluteUri(safePath + separator + "checkout=cancel").ToString()
                });

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var error =
                    JsonSerializer.Deserialize<
                        CheckoutErrorResponse>(json);

                throw new InvalidOperationException(
                    error?.Error ??
                    "Unable to begin checkout.");
            }
            catch (JsonException)
            {
                throw new InvalidOperationException(
                    $"Unable to begin checkout. {json}");
            }
        }

        var checkout =
            JsonSerializer.Deserialize<
                CheckoutResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (string.IsNullOrWhiteSpace(
            checkout?.CheckoutUrl))
        {
            throw new InvalidOperationException(
                "Stripe did not return a Checkout URL.");
        }

        return checkout.CheckoutUrl;
    }


    private sealed class CheckoutResponse
    {
        [JsonPropertyName("checkoutUrl")]
        public string? CheckoutUrl { get; set; }
    }


    private sealed class CheckoutErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
