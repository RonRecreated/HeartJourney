using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HeartJourneyWeb.Services.Auth;
using HeartJourneyWeb.Services.Supabase;
using Microsoft.Extensions.Options;
using Supabase;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.ActionSteps;

public class UserActionStepService : IUserActionStepService
{
    private readonly Client _supabaseClient;
    private readonly HttpClient _httpClient;
    private readonly AppSupabaseOptions _options;
    private readonly IAuthService _authService;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public UserActionStepService(
        HttpClient httpClient,
        IOptions<AppSupabaseOptions> options,
        IAuthService authService,
        Client supabaseClient)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _authService = authService;
        _supabaseClient = supabaseClient;
    }

    public async Task<IReadOnlyList<UserActionStepRecord>> GetActionStepsForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default)
    {
        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return Array.Empty<UserActionStepRecord>();
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/user_action_steps" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            $"&dimension_slug=eq.{Uri.EscapeDataString(dimensionSlug)}" +
            "&order=created_at.asc";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load user action steps. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var records = await response.Content.ReadFromJsonAsync<List<UserActionStepRecord>>(
            _jsonOptions,
            cancellationToken);

        return records ?? new List<UserActionStepRecord>();
    }

    public async Task<UserActionStepRecord> SaveActionStepStatusAsync(
        SaveUserActionStepRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("You must be signed in to update an action step.");
        }

        var existing = await GetExistingActionStepAsync(
            request.JourneySlug,
            request.MilestoneSlug,
            request.DimensionSlug,
            request.ActionStepKey,
            cancellationToken);

        return existing is null
            ? await CreateActionStepAsync(request, cancellationToken)
            : await UpdateActionStepAsync(request, cancellationToken);
    }

    public async Task DeleteActionStepsForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default)
    {
        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return;
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/user_action_steps" +
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
                $"Unable to delete user action steps. Status: {(int)response.StatusCode}. Response: {error}");
        }
    }

    private async Task<UserActionStepRecord?> GetExistingActionStepAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        string actionStepKey,
        CancellationToken cancellationToken = default)
    {
        var requestUrl =
            $"{_options.Url}/rest/v1/user_action_steps" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId!)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            $"&dimension_slug=eq.{Uri.EscapeDataString(dimensionSlug)}" +
            $"&action_step_key=eq.{Uri.EscapeDataString(actionStepKey)}" +
            "&select=*";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to check user action step. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var records = await response.Content.ReadFromJsonAsync<List<UserActionStepRecord>>(
            _jsonOptions,
            cancellationToken);

        return records?.FirstOrDefault();
    }

    private async Task<UserActionStepRecord> CreateActionStepAsync(
        SaveUserActionStepRequest requestModel,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var record = new UserActionStepRecord
        {
            UserId = _authService.UserId!,
            JourneySlug = requestModel.JourneySlug,
            MilestoneSlug = requestModel.MilestoneSlug,
            DimensionSlug = requestModel.DimensionSlug,
            ActionStepKey = requestModel.ActionStepKey,
            ActionStepTitle = requestModel.ActionStepTitle,
            ActionStepCategory = requestModel.ActionStepCategory,
            ReflectionPromptId = requestModel.ReflectionPromptId,
            SelectedAnswerLabel = requestModel.SelectedAnswerLabel,
            Status = requestModel.Status,
            CreatedAt = now,
            UpdatedAt = now,
            CompletedAt = requestModel.Status == "completed" ? now : (DateTime?)null
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/user_action_steps" +
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
                $"Unable to create user action step. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var createdRecords = await response.Content.ReadFromJsonAsync<List<UserActionStepRecord>>(
            _jsonOptions,
            cancellationToken);

        return createdRecords?.FirstOrDefault()
            ?? throw new InvalidOperationException("User action step was created, but Supabase returned no data.");
    }

    private async Task<UserActionStepRecord> UpdateActionStepAsync(
        SaveUserActionStepRequest requestModel,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var patchRecord = new
        {
            action_step_title = requestModel.ActionStepTitle,
            action_step_category = requestModel.ActionStepCategory,
            reflection_prompt_id = requestModel.ReflectionPromptId,
            selected_answer_label = requestModel.SelectedAnswerLabel,
            status = requestModel.Status,
            updated_at = now,
            completed_at = requestModel.Status == "completed" ? now : (DateTime?)null
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/user_action_steps" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId!)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(requestModel.JourneySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(requestModel.MilestoneSlug)}" +
            $"&dimension_slug=eq.{Uri.EscapeDataString(requestModel.DimensionSlug)}" +
            $"&action_step_key=eq.{Uri.EscapeDataString(requestModel.ActionStepKey)}" +
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
                $"Unable to update user action step. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var updatedRecords = await response.Content.ReadFromJsonAsync<List<UserActionStepRecord>>(
            _jsonOptions,
            cancellationToken);

        return updatedRecords?.FirstOrDefault()
            ?? throw new InvalidOperationException("User action step was updated, but Supabase returned no data.");
    }

    private void AddSupabaseHeaders(HttpRequestMessage request)
    {
        var accessToken = _supabaseClient.Auth.CurrentSession?.AccessToken;

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        }

        request.Headers.TryAddWithoutValidation("apikey", _options.Key);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}