using System.Net.Http.Headers;
using HeartJourneyWeb.Services.Auth;
using Microsoft.Extensions.Options;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.MilestoneReset;

public class MilestoneResetService : IMilestoneResetService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly AppSupabaseOptions _options;

    public MilestoneResetService(
        HttpClient httpClient,
        IAuthService authService,
        IOptions<AppSupabaseOptions> options)
    {
        _httpClient = httpClient;
        _authService = authService;
        _options = options.Value;
    }

    public async Task ResetMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn ||
            string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException(
                "A signed-in user is required to reset a milestone.");
        }

        // Order is intentional.
        // Child/user-work records first, milestone progress last.

        await DeleteMilestoneRecordsAsync(
            "user_action_steps",
            journeySlug,
            milestoneSlug,
            cancellationToken);

        await DeleteMilestoneRecordsAsync(
            "reflection_answers",
            journeySlug,
            milestoneSlug,
            cancellationToken);

        await DeleteMilestoneRecordsAsync(
            "dimension_progress",
            journeySlug,
            milestoneSlug,
            cancellationToken);

        await DeleteMilestoneRecordsAsync(
            "milestone_progress",
            journeySlug,
            milestoneSlug,
            cancellationToken);
    }

    private async Task DeleteMilestoneRecordsAsync(
        string tableName,
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken)
    {
        var requestUrl =
            $"{_options.Url}/rest/v1/{tableName}" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId!)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}";

        using var request =
            new HttpRequestMessage(HttpMethod.Delete, requestUrl);

        await AddSupabaseHeadersAsync(request);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to reset milestone data from {tableName}. " +
                $"Status: {(int)response.StatusCode}. Response: {error}");
        }
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
                "Your session has expired. Please sign in again.");
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }
}