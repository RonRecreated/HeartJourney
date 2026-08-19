using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HeartJourneyWeb.Services.Auth;
using Microsoft.Extensions.Options;
using Supabase;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;
using System.Text.Json.Serialization;

namespace HeartJourneyWeb.Services.ReflectionAnswers;

public class ReflectionAnswerService : IReflectionAnswerService
{
    private readonly Client _supabaseClient;
    private readonly IAuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly AppSupabaseOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ReflectionAnswerService(
        Client supabaseClient,
        IAuthService authService,
        HttpClient httpClient,
        IOptions<AppSupabaseOptions> options)
    {
        _supabaseClient = supabaseClient;
        _authService = authService;
        _httpClient = httpClient;
        _options = options.Value;
    }

    // Loads one saved answer
    public async Task<ReflectionAnswerRecord?> GetAnswerForPromptAsync(
        string reflectionPromptId,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return null;
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/reflection_answers" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&reflection_prompt_id=eq.{Uri.EscapeDataString(reflectionPromptId)}" +
            $"&select=*";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load reflection answer. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var answers = await response.Content.ReadFromJsonAsync<List<ReflectionAnswerRecord>>(
            _jsonOptions,
            cancellationToken);

        return answers?.FirstOrDefault();
    }

    public async Task<ReflectionAnswerRecord> SaveAnswerAsync(
        SaveReflectionAnswerRequest requestModel,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("A signed-in user is required to save a reflection answer.");
        }

        var existingAnswer = await GetAnswerForPromptAsync(
            requestModel.ReflectionPromptId,
            cancellationToken);

        if (existingAnswer is null)
        {
            return await InsertAnswerAsync(requestModel, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(existingAnswer.Id))
        {
            throw new InvalidOperationException(
                "Unable to update reflection answer because the saved answer id was not returned from Supabase.");
        }

        return await UpdateAnswerAsync(requestModel, cancellationToken);
    }

    private async Task<ReflectionAnswerRecord> InsertAnswerAsync(
        SaveReflectionAnswerRequest requestModel,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var record = new ReflectionAnswerRecord
        {
            UserId = _authService.UserId!,
            JourneySlug = requestModel.JourneySlug,
            MilestoneSlug = requestModel.MilestoneSlug,
            DimensionSlug = requestModel.DimensionSlug,
            ReflectionPromptId = requestModel.ReflectionPromptId,
            ReflectionPromptSlug = requestModel.ReflectionPromptSlug,
            ReflectionPromptTitle = requestModel.ReflectionPromptTitle,
            SelectedAnswerLabel = requestModel.SelectedAnswerLabel,
            SelectedAnswerDescription = requestModel.SelectedAnswerDescription,
            SelectedStatus = requestModel.SelectedStatus,
            SelectedConcernLevel = requestModel.SelectedConcernLevel,
            GuidanceMessage = requestModel.GuidanceMessage,
            Notes = requestModel.Notes,
            AnsweredAt = now,
            UpdatedAt = now
        };

        var requestUrl = $"{_options.Url}/rest/v1/reflection_answers";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        AddSupabaseHeaders(request);
        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");
        request.Content = JsonContent.Create(record, options: _jsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to save reflection answer. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var createdAnswers = await response.Content.ReadFromJsonAsync<List<ReflectionAnswerRecord>>(
            _jsonOptions,
            cancellationToken);

        return createdAnswers?.FirstOrDefault()
            ?? throw new InvalidOperationException("Reflection answer was saved, but Supabase returned no data.");
    }

    private async Task<ReflectionAnswerRecord> UpdateAnswerAsync(
    SaveReflectionAnswerRequest requestModel,
    CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("You must be signed in to update a reflection answer.");
        }

        if (string.IsNullOrWhiteSpace(requestModel.ReflectionPromptId))
        {
            throw new InvalidOperationException("Reflection prompt id is required to update an answer.");
        }

        var patchRecord = new
        {
            selected_answer_label = requestModel.SelectedAnswerLabel,
            selected_answer_description = requestModel.SelectedAnswerDescription,
            selected_status = requestModel.SelectedStatus,
            selected_concern_level = requestModel.SelectedConcernLevel,
            guidance_message = requestModel.GuidanceMessage,
            notes = requestModel.Notes,
            updated_at = DateTime.UtcNow
        };

        var requestUrl =
            $"{_options.Url}/rest/v1/reflection_answers" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&reflection_prompt_id=eq.{Uri.EscapeDataString(requestModel.ReflectionPromptId)}";

        using var request = new HttpRequestMessage(HttpMethod.Patch, requestUrl);

        AddSupabaseHeaders(request);

        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");

        request.Content = JsonContent.Create(
            patchRecord,
            options: _jsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to update reflection answer. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var updatedAnswers = await response.Content.ReadFromJsonAsync<List<ReflectionAnswerRecord>>(
            _jsonOptions,
            cancellationToken);

        return updatedAnswers?.FirstOrDefault()
            ?? throw new InvalidOperationException("Reflection answer was updated, but Supabase returned no data.");
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

    // Resume at first unanswered question
    public async Task<IReadOnlyList<ReflectionAnswerRecord>> GetAnswersForPromptsAsync(
    IReadOnlyList<string> reflectionPromptIds,
    CancellationToken cancellationToken = default)
    {
    await _authService.InitializeAsync();

    if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
    {
        return Array.Empty<ReflectionAnswerRecord>();
    }

    var cleanPromptIds = reflectionPromptIds
        .Where(id => !string.IsNullOrWhiteSpace(id))
        .Distinct()
        .ToList();

    if (cleanPromptIds.Count == 0)
    {
        return Array.Empty<ReflectionAnswerRecord>();
    }

    var promptIdList = string.Join(
        ",",
        cleanPromptIds.Select(id => $"\"{id.Replace("\"", "\\\"")}\""));

    var requestUrl =
        $"{_options.Url}/rest/v1/reflection_answers" +
        $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
        $"&reflection_prompt_id=in.({Uri.EscapeDataString(promptIdList)})" +
        $"&select=*";

    using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
    AddSupabaseHeaders(request);

    using var response = await _httpClient.SendAsync(request, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new InvalidOperationException(
            $"Unable to load reflection answers. Status: {(int)response.StatusCode}. Response: {error}");
    }

    var answers = await response.Content.ReadFromJsonAsync<List<ReflectionAnswerRecord>>(
        _jsonOptions,
        cancellationToken);

    return answers ?? new List<ReflectionAnswerRecord>();
    }

    public async Task<IReadOnlyList<ReflectionAnswerRecord>> GetAnswersForMilestoneAsync(
        string journeySlug,
        string milestoneSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return Array.Empty<ReflectionAnswerRecord>();
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/reflection_answers" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            $"&select=*";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load milestone reflection answers. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var answers = await response.Content.ReadFromJsonAsync<List<ReflectionAnswerRecord>>(
            _jsonOptions,
            cancellationToken);

        return answers ?? new List<ReflectionAnswerRecord>();
    }

    public async Task DeleteAnswersForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("You must be signed in to reset reflection answers.");
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/reflection_answers" +
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
                $"Unable to reset reflection answers. Status: {(int)response.StatusCode}. Response: {error}");
        }
    }

    public async Task<IReadOnlyList<ReflectionAnswerRecord>> GetAnswersForDimensionAsync(
        string journeySlug,
        string milestoneSlug,
        string dimensionSlug,
        CancellationToken cancellationToken = default)
    {
        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            return Array.Empty<ReflectionAnswerRecord>();
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/reflection_answers" +
            $"?user_id=eq.{Uri.EscapeDataString(_authService.UserId)}" +
            $"&journey_slug=eq.{Uri.EscapeDataString(journeySlug)}" +
            $"&milestone_slug=eq.{Uri.EscapeDataString(milestoneSlug)}" +
            $"&dimension_slug=eq.{Uri.EscapeDataString(dimensionSlug)}" +
            "&order=answered_at.asc";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load reflection answers for dimension. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var answers = await response.Content.ReadFromJsonAsync<List<ReflectionAnswerRecord>>(
            _jsonOptions,
            cancellationToken);

        return answers ?? new List<ReflectionAnswerRecord>();
    }
}