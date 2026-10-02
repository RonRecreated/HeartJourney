using System.Net.Http.Headers;
using System.Text.Json;
using HeartJourneyWeb.Services.Auth;
using Microsoft.Extensions.Options;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.JourneyAccess;

public class JourneyAccessService : IJourneyAccessService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly AppSupabaseOptions _options;

    public JourneyAccessService(
        HttpClient httpClient,
        IAuthService authService,
        IOptions<AppSupabaseOptions> options)
    {
        _httpClient = httpClient;
        _authService = authService;
        _options = options.Value;
    }

    public async Task<bool> HasAccessAsync(
        string journeySlug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(journeySlug))
        {
            return false;
        }

        var accessToken =
            await _authService.GetValidAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        var url =
            $"{_options.Url.TrimEnd('/')}" +
            "/rest/v1/journey_access" +
            $"?journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            "&is_active=eq.true" +
            "&select=id" +
            "&limit=1";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddSupabaseHeadersAsync(request);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Unable to check Journey access. {error}");
        }

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        var records =
            JsonSerializer.Deserialize<
                List<JourneyAccessRecord>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        return records is { Count: > 0 };
    }


    public async Task<IReadOnlyList<JourneyAccessRecord>>
        GetUserAccessAsync(
            CancellationToken cancellationToken = default)
    {
        var accessToken =
            await _authService.GetValidAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Array.Empty<JourneyAccessRecord>();
        }

        var url =
            $"{_options.Url.TrimEnd('/')}" +
            "/rest/v1/journey_access" +
            "?is_active=eq.true" +
            "&select=*" +
            "&order=granted_at.desc";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        await AddSupabaseHeadersAsync(request);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load Journey access. {error}");
        }

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        return
            JsonSerializer.Deserialize<
                List<JourneyAccessRecord>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? [];
    }


    private async Task AddSupabaseHeadersAsync(
        HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation(
            "apikey",
            _options.Key);

        var accessToken =
            await _authService.GetValidAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "Your session has expired. Please refresh the page.");
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }
}