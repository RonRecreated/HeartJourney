using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HeartJourneyWeb.Services.Auth;
using Microsoft.Extensions.Options;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.MilestoneProgress;

public class MilestoneProgressService : IMilestoneProgressService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly AppSupabaseOptions _options;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public MilestoneProgressService(
        HttpClient httpClient,
        IAuthService authService,
        IOptions<AppSupabaseOptions> options)
    {
        _httpClient = httpClient;
        _authService = authService;
        _options = options.Value;
    }

    public async Task<MilestoneProgressRecord?> GetProgressAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn ||
            string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return null;
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/milestone_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            "&select=*";

        using var request =
            new HttpRequestMessage(HttpMethod.Get, requestUrl);

        await AddSupabaseHeadersAsync(request);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load milestone progress. " +
                $"Status: {(int)response.StatusCode}. Response: {error}");
        }

        var progress =
            await response.Content.ReadFromJsonAsync<List<MilestoneProgressRecord>>(
                _jsonOptions,
                cancellationToken);

        return progress?.FirstOrDefault();
    }

    public async Task<IReadOnlyList<MilestoneProgressRecord>>
        GetProgressForJourneyAsync(
            string journeySlug,
            CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn ||
            string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return Array.Empty<MilestoneProgressRecord>();
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/milestone_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            "&select=*";

        using var request =
            new HttpRequestMessage(HttpMethod.Get, requestUrl);

        await AddSupabaseHeadersAsync(request);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load milestone progress for journey. " +
                $"Status: {(int)response.StatusCode}. Response: {error}");
        }

        var progress =
            await response.Content.ReadFromJsonAsync<List<MilestoneProgressRecord>>(
                _jsonOptions,
                cancellationToken);

        return progress ?? new List<MilestoneProgressRecord>();
    }

    public async Task<MilestoneProgressRecord> StartMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn ||
            string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException(
                "A signed-in user is required to start a milestone.");
        }

        var existingProgress = await GetProgressAsync(
            journeySlug,
            milestoneSlug,
            cancellationToken);

        if (existingProgress is not null)
        {
            return await UpdateLastVisitedAsync(
                journeySlug,
                milestoneSlug,
                cancellationToken);
        }

        var now = DateTime.UtcNow;

        var newProgress = new MilestoneProgressRecord
        {
            UserId = _authService.UserId,
            JourneySlug = journeySlug,
            MilestoneSlug = milestoneSlug,
            IntroViewCount = 0,
            StartedAt = now,
            LastVisitedAt = now,
            UpdatedAt = now
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/milestone_progress" +
            "?select=*";

        using var request =
            new HttpRequestMessage(HttpMethod.Post, requestUrl);

        await AddSupabaseHeadersAsync(request);

        request.Headers.TryAddWithoutValidation(
            "Prefer",
            "return=representation");

        request.Content =
            JsonContent.Create(newProgress, options: _jsonOptions);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to start milestone. " +
                $"Status: {(int)response.StatusCode}. Response: {error}");
        }

        var createdProgress =
            await response.Content.ReadFromJsonAsync<List<MilestoneProgressRecord>>(
                _jsonOptions,
                cancellationToken);

        return createdProgress?.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Milestone progress was created, but Supabase returned no data.");
    }

    public async Task<MilestoneProgressRecord> CompleteIntroVisitAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        var progress = await GetProgressAsync(
            journeySlug,
            milestoneSlug,
            cancellationToken);

        if (progress is null)
        {
            progress = await StartMilestoneAsync(
                journeySlug,
                milestoneSlug,
                cancellationToken);
        }

        var now = DateTime.UtcNow;

        var patchRecord = new
        {
            intro_view_count = progress.IntroViewCount + 1,
            last_visited_at = now,
            updated_at = now
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/milestone_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId!)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            "&select=*";

        using var request =
            new HttpRequestMessage(HttpMethod.Patch, requestUrl);

        await AddSupabaseHeadersAsync(request);

        request.Headers.TryAddWithoutValidation(
            "Prefer",
            "return=representation");

        request.Content =
            JsonContent.Create(patchRecord, options: _jsonOptions);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to complete milestone intro visit. " +
                $"Status: {(int)response.StatusCode}. Response: {error}");
        }

        var updatedProgress =
            await response.Content.ReadFromJsonAsync<List<MilestoneProgressRecord>>(
                _jsonOptions,
                cancellationToken);

        return updatedProgress?.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Milestone progress was updated, but Supabase returned no data.");
    }

    private async Task<MilestoneProgressRecord> UpdateLastVisitedAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var patchRecord = new
        {
            last_visited_at = now,
            updated_at = now
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/milestone_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId!)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            "&select=*";

        using var request =
            new HttpRequestMessage(HttpMethod.Patch, requestUrl);

        await AddSupabaseHeadersAsync(request);

        request.Headers.TryAddWithoutValidation(
            "Prefer",
            "return=representation");

        request.Content =
            JsonContent.Create(patchRecord, options: _jsonOptions);

        using var response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to update milestone visit. " +
                $"Status: {(int)response.StatusCode}. Response: {error}");
        }

        var updatedProgress =
            await response.Content.ReadFromJsonAsync<List<MilestoneProgressRecord>>(
                _jsonOptions,
                cancellationToken);

        return updatedProgress?.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Milestone progress was updated, but Supabase returned no data.");
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