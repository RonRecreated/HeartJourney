using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HeartJourneyWeb.Services.Auth;
using Microsoft.Extensions.Options;
using Supabase;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.DimensionProgress;

public class DimensionProgressService : IDimensionProgressService
{
    private readonly Client _supabaseClient;
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly AppSupabaseOptions _options;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public DimensionProgressService(
        Client supabaseClient,
        HttpClient httpClient,
        IAuthService authService,
        IOptions<AppSupabaseOptions> options)
    {
        _supabaseClient = supabaseClient;
        _httpClient = httpClient;
        _authService = authService;
        _options = options.Value;
    }

    public async Task<DimensionProgressRecord?> GetProgressAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return null;
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/dimension_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            $"&dimension_slug=eq.{Uri.EscapeDataString(dimensionSlug)}" +
            "&select=*";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load dimension progress. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var progress = await response.Content.ReadFromJsonAsync<List<DimensionProgressRecord>>(
            _jsonOptions,
            cancellationToken);

        return progress?.FirstOrDefault();
    }

    public async Task<IReadOnlyList<DimensionProgressRecord>> GetProgressForMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return Array.Empty<DimensionProgressRecord>();
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/dimension_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            "&select=*";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load milestone dimension progress. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var progress = await response.Content.ReadFromJsonAsync<List<DimensionProgressRecord>>(
            _jsonOptions,
            cancellationToken);

        return progress ?? new List<DimensionProgressRecord>();
    }

    public async Task<DimensionProgressRecord> SaveProgressAsync(
        SaveDimensionProgressRequest requestModel,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("You must be signed in to save dimension progress.");
        }

        var existingProgress = await GetProgressAsync(
            requestModel.JourneySlug,
            requestModel.MilestoneSlug,
            requestModel.DimensionSlug,
            cancellationToken);

        if (existingProgress is null)
        {
            return await CreateProgressAsync(requestModel, cancellationToken);
        }

        return await UpdateProgressAsync(requestModel, cancellationToken);
    }

    public async Task DeleteProgressForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("You must be signed in to delete dimension progress.");
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/dimension_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            $"&dimension_slug=eq.{Uri.EscapeDataString(dimensionSlug)}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to delete dimension progress. Status: {(int)response.StatusCode}. Response: {error}");
        }
    }

    private async Task<DimensionProgressRecord> CreateProgressAsync(
        SaveDimensionProgressRequest requestModel,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var record = new DimensionProgressRecord
        {
            UserId = _authService.UserId!,
            JourneySlug = requestModel.JourneySlug,
            MilestoneSlug = requestModel.MilestoneSlug,
            DimensionSlug = requestModel.DimensionSlug,
            Status = requestModel.Status,
            CurrentPromptId = requestModel.CurrentPromptId,
            CurrentPromptSlug = requestModel.CurrentPromptSlug,
            VisibleQuestionNumber = requestModel.VisibleQuestionNumber,
            PromptHistoryJson = requestModel.PromptHistoryJson,
            OutcomeKey = requestModel.OutcomeKey,
            OutcomeMessage = requestModel.OutcomeMessage,
            RecommendedMilestoneSlug = requestModel.RecommendedMilestoneSlug,
            RecommendedMilestoneTitle = requestModel.RecommendedMilestoneTitle,
            CompletedAt = requestModel.CompletedAt,
            StartedAt = now,
            UpdatedAt = now
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/dimension_progress" +
            "?select=*";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        AddSupabaseHeaders(request);

        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");

        request.Content = JsonContent.Create(record, options: _jsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to create dimension progress. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var createdProgress = await response.Content.ReadFromJsonAsync<List<DimensionProgressRecord>>(
            _jsonOptions,
            cancellationToken);

        return createdProgress?.FirstOrDefault()
            ?? throw new InvalidOperationException("Dimension progress was created, but Supabase returned no data.");
    }

    private async Task<DimensionProgressRecord> UpdateProgressAsync(
        SaveDimensionProgressRequest requestModel,
        CancellationToken cancellationToken = default)
    {
        var patchRecord = new
        {
            status = requestModel.Status,
            current_prompt_id = requestModel.CurrentPromptId,
            current_prompt_slug = requestModel.CurrentPromptSlug,
            outcome_key = requestModel.OutcomeKey,
            outcome_message = requestModel.OutcomeMessage,
            visible_question_number = requestModel.VisibleQuestionNumber,
            prompt_history_json = requestModel.PromptHistoryJson,
            recommended_milestone_slug = requestModel.RecommendedMilestoneSlug,
            recommended_milestone_title = requestModel.RecommendedMilestoneTitle,
            completed_at = requestModel.CompletedAt,
            updated_at = DateTime.UtcNow
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/dimension_progress" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId!)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(requestModel.JourneySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(requestModel.MilestoneSlug)}" +
            $"&dimension_slug=eq.{Uri.EscapeDataString(requestModel.DimensionSlug)}" +
            "&select=*";

        using var request = new HttpRequestMessage(HttpMethod.Patch, requestUrl);
        AddSupabaseHeaders(request);

        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");

        request.Content = JsonContent.Create(patchRecord, options: _jsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to update dimension progress. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var updatedProgress = await response.Content.ReadFromJsonAsync<List<DimensionProgressRecord>>(
            _jsonOptions,
            cancellationToken);

        return updatedProgress?.FirstOrDefault()
            ?? throw new InvalidOperationException("Dimension progress was updated, but Supabase returned no data.");
    }

    private void AddSupabaseHeaders(HttpRequestMessage request)
    {
        var accessToken = _supabaseClient.Auth.CurrentSession?.AccessToken;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("The signed-in user session is missing an access token.");
        }

        request.Headers.TryAddWithoutValidation("apikey", _options.Key);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
