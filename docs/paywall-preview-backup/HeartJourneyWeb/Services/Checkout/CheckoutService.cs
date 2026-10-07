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

    public CheckoutService(
        HttpClient httpClient,
        IAuthService authService,
        IOptions<AppSupabaseOptions> options)
    {
        _httpClient = httpClient;
        _authService = authService;
        _options = options.Value;
    }

    public async Task<string> CreateCheckoutAsync(
        string journeySlug,
        CancellationToken cancellationToken = default)
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

        request.Content =
            JsonContent.Create(
                new
                {
                    journeySlug
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