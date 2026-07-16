using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HeartJourneyWeb.Services.Auth;
using Microsoft.Extensions.Options;
using Supabase;
using AppSupabaseOptions = HeartJourneyWeb.Services.Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.Profiles;

public class ProfileService : IProfileService
{
    private readonly Client _supabaseClient;
    private readonly IAuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly AppSupabaseOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProfileService(
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

    public HeartJourneyProfile? CurrentProfile { get; private set; }

    public async Task<HeartJourneyProfile?> GetProfileAsync(
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            CurrentProfile = null;
            return null;
        }

        var requestUrl =
            $"{_options.Url}/rest/v1/profiles?id=eq.{Uri.EscapeDataString(_authService.UserId)}&select=*";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        AddSupabaseHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to load profile. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var profiles = await response.Content.ReadFromJsonAsync<List<HeartJourneyProfile>>(
            _jsonOptions,
            cancellationToken);

        CurrentProfile = profiles?.FirstOrDefault();

        return CurrentProfile;
    }

    public async Task<HeartJourneyProfile> EnsureProfileAsync(
        CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("A signed-in user is required to create or load a profile.");
        }

        var existingProfile = await GetProfileAsync(cancellationToken);

        if (existingProfile is not null)
        {
            return existingProfile;
        }

        var newProfile = new HeartJourneyProfile
        {
            Id = _authService.UserId,
            Email = _authService.Email,
            OnboardingCompleted = false
        };

        var requestUrl = $"{_options.Url}/rest/v1/profiles";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        AddSupabaseHeaders(request);
        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");
        request.Content = JsonContent.Create(newProfile);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to create profile. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var createdProfiles = await response.Content.ReadFromJsonAsync<List<HeartJourneyProfile>>(
            _jsonOptions,
            cancellationToken);

        CurrentProfile = createdProfiles?.FirstOrDefault()
            ?? throw new InvalidOperationException("Profile was created, but Supabase returned no profile data.");

        return CurrentProfile;
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

    public async Task<HeartJourneyProfile> UpdateProfileAsync(
    HeartJourneyProfile profile,
    CancellationToken cancellationToken = default)
    {
        await _authService.InitializeAsync();

        if (!_authService.IsSignedIn || string.IsNullOrWhiteSpace(_authService.UserId))
        {
            throw new InvalidOperationException("A signed-in user is required to update a profile.");
        }

        if (!string.Equals(profile.Id, _authService.UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("You can only update your own profile.");
        }

        profile.UpdatedAt = DateTime.UtcNow;

        var requestUrl =
            $"{_options.Url}/rest/v1/profiles?id=eq.{Uri.EscapeDataString(profile.Id)}";

        using var request = new HttpRequestMessage(HttpMethod.Patch, requestUrl);
        AddSupabaseHeaders(request);
        request.Headers.TryAddWithoutValidation("Prefer", "return=representation");
        request.Content = JsonContent.Create(profile);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"Unable to update profile. Status: {(int)response.StatusCode}. Response: {error}");
        }

        var updatedProfiles = await response.Content.ReadFromJsonAsync<List<HeartJourneyProfile>>(
            _jsonOptions,
            cancellationToken);

        CurrentProfile = updatedProfiles?.FirstOrDefault()
            ?? throw new InvalidOperationException("Profile was updated, but Supabase returned no profile data.");

        return CurrentProfile;
    }
}