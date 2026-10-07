using Supabase;
using System.Text;
using System.Text.Json;
using HeartJourneyWeb.Services.BrowserStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace HeartJourneyWeb.Services.Auth;

public class SupabaseAuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;
    private readonly Client _supabaseClient;
    private bool _isInitialized;

    private const string AuthSessionKey = "heartjourney.auth.session";

    private readonly BrowserStorageService _browserStorage;
    private readonly NavigationManager _navigationManager;

    public SupabaseAuthService(
        Client supabaseClient, 
        BrowserStorageService browserStorage, 
        NavigationManager navigationManager,
        HttpClient http,
        IConfiguration configuration)
    {
        _supabaseClient = supabaseClient;
        _browserStorage = browserStorage;
        _navigationManager = navigationManager;
        _http = http;
        _configuration = configuration;
    }

    public event Action? AuthStateChanged;

    public bool IsSignedIn => _supabaseClient.Auth.CurrentSession is not null;

    public string? UserId => _supabaseClient.Auth.CurrentUser?.Id;

    public string? Email => _supabaseClient.Auth.CurrentUser?.Email;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _supabaseClient.InitializeAsync();

        var savedSession = await _browserStorage.GetAsync<PersistedAuthSession>(
            AuthSessionKey);

        if (savedSession is not null
            && !string.IsNullOrWhiteSpace(savedSession.AccessToken)
            && !string.IsNullOrWhiteSpace(savedSession.RefreshToken))
        {
            try
            {
                await _supabaseClient.Auth.SetSession(
                    savedSession.AccessToken,
                    savedSession.RefreshToken);
            }
            catch
            {
                // The saved token pair is no longer valid.
                // Remove it so the app does not keep trying to restore it.
                await _browserStorage.RemoveAsync(AuthSessionKey);
            }
        }

        _isInitialized = true;

        NotifyAuthStateChanged();
    }

    public async Task<AuthResult> SignUpAsync(
        string email,
        string password,
        string captchaToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var redirectTo = Uri.EscapeDataString(
                _navigationManager
                    .ToAbsoluteUri("/auth/confirmed")
                    .ToString());

            using var response = await PostAuthAsync(
                $"signup?redirect_to={redirectTo}",
                email,
                password,
                captchaToken,
                cancellationToken);

            NotifyAuthStateChanged();

            return AuthResult.Success(
                "Account created. Please check your email to confirm your account, then sign in.");
        }
        catch (Exception ex)
        {
            return AuthResult.Failure(GetFriendlyErrorMessage(ex));
        }
    }

    public async Task<AuthResult> SignInAsync(
        string email,
        string password,
        string captchaToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await PostAuthAsync(
                "token?grant_type=password",
                email,
                password,
                captchaToken,
                cancellationToken);

            var accessToken = response.RootElement
                .GetProperty("access_token")
                .GetString();

            var refreshToken = response.RootElement
                .GetProperty("refresh_token")
                .GetString();

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return AuthResult.Failure(
                    "Sign in did not return a valid session.");
            }

            var session = await _supabaseClient.Auth.SetSession(accessToken, refreshToken);

            if (session is null)
            {
                return AuthResult.Failure("Sign in failed. Please check your email and password.");
            }

            if (_supabaseClient.Auth.CurrentSession is not null)
            {
                await _browserStorage.SetAsync(
                    AuthSessionKey,
                    new PersistedAuthSession
                    {
                        AccessToken = _supabaseClient.Auth.CurrentSession.AccessToken ?? string.Empty,
                        RefreshToken = _supabaseClient.Auth.CurrentSession.RefreshToken ?? string.Empty
                    });
            }

            NotifyAuthStateChanged();

            return AuthResult.Success("Signed in successfully.");
        }
        catch (Exception ex)
        {
            return AuthResult.Failure(GetFriendlyErrorMessage(ex));
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            await _supabaseClient.Auth.SignOut();
        }
        finally
        {
            await _browserStorage.RemoveAsync(AuthSessionKey);

            NotifyAuthStateChanged();
        }
    }

    private void NotifyAuthStateChanged()
    {
        AuthStateChanged?.Invoke();
    }

    private static string GetFriendlyErrorMessage(Exception ex)
    {
        var message = ex.Message;

        if (message.Contains("Invalid login credentials", StringComparison.OrdinalIgnoreCase))
        {
            return "The email or password is incorrect.";
        }

        if (message.Contains("Email not confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return "Please confirm your email before signing in.";
        }

        if (message.Contains("User already registered", StringComparison.OrdinalIgnoreCase))
        {
            return "An account already exists for this email address.";
        }

        return message;
    }

    public async Task<string?> GetValidAccessTokenAsync()
    {
        var session = _supabaseClient.Auth.CurrentSession;

        if (session is null ||
            string.IsNullOrWhiteSpace(session.AccessToken) ||
            string.IsNullOrWhiteSpace(session.RefreshToken))
        {
            return null;
        }

        var accessToken = session.AccessToken;
        
        // If the token is still comfortably valid, just use it.
        if (!IsJwtNearExpiration(accessToken, TimeSpan.FromMinutes(5)))
        {
            return accessToken;
        }

        try
        {
            var refreshedSession = await _supabaseClient.Auth.SetSession(
                session.AccessToken,
                session.RefreshToken,
            true);

            if (refreshedSession is null ||
                string.IsNullOrWhiteSpace(refreshedSession.AccessToken))
            {
                return null;
            }

            // IMPORTANT:
            // Save the NEW token pair back to browser localStorage.
            await _browserStorage.SetAsync(
                AuthSessionKey,
                new PersistedAuthSession
                {
                    AccessToken = refreshedSession.AccessToken ?? string.Empty,
                    RefreshToken = refreshedSession.RefreshToken ?? string.Empty
                });

            return refreshedSession.AccessToken;
        }
        catch
        {
            // Do NOT return the old access token here.
            // If it has expired, that just causes another JWT expired response.
            return null;
        }
    }

    private static bool IsJwtNearExpiration(
        string accessToken,
        TimeSpan refreshBeforeExpiration)
    {
        try
        {
            var parts = accessToken.Split('.');

            if (parts.Length < 2)
            {
                return true;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;

                case 3:
                    payload += "=";
                    break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("exp", out var expElement))
            {
                return true;
            }

            var expirationUnixSeconds = expElement.GetInt64();

            var expirationTime =
                DateTimeOffset.FromUnixTimeSeconds(expirationUnixSeconds);

            return expirationTime <=
                DateTimeOffset.UtcNow.Add(refreshBeforeExpiration);
        }
        catch
        {
            // If we cannot read the token safely,
            // refresh it rather than risk sending an expired JWT.
            return true;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(
        string email,
        string captchaToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var options =
            new global::Supabase.Gotrue.ResetPasswordForEmailOptions(email)
            {
                CaptchaToken = captchaToken,
                
                RedirectTo = _navigationManager
                    .ToAbsoluteUri("/update-password")
                    .ToString(),
            };

        await _supabaseClient.Auth.ResetPasswordForEmail(options);

        return true;
    }

    public async Task<bool> UpdatePasswordAsync(
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        var attributes = new global::Supabase.Gotrue.UserAttributes
        {
            Password = newPassword
        };

        var user = await _supabaseClient.Auth.Update(attributes);

        return user is not null;
    }

    public async Task<bool> EstablishRecoverySessionAsync(
        string callbackUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            return false;
        }

        try
        {
            var callbackUri = new Uri(callbackUrl);

            var session = await _supabaseClient.Auth.GetSessionFromUrl(
                callbackUri,
                storeSession: true);

            if (session is null ||
                string.IsNullOrWhiteSpace(session.AccessToken) ||
                string.IsNullOrWhiteSpace(session.RefreshToken))
            {
                return false;
            }

            await _browserStorage.SetAsync(
                AuthSessionKey,
                new PersistedAuthSession
                {
                    AccessToken = session.AccessToken ?? string.Empty,
                    RefreshToken = session.RefreshToken ?? string.Empty
                });

            NotifyAuthStateChanged();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ClearLocalSessionAsync()
    {
        await _browserStorage.RemoveAsync(AuthSessionKey);

        NotifyAuthStateChanged();
    }

    private async Task<JsonDocument> PostAuthAsync(
        string endpoint,
        string email,
        string password,
        string captchaToken,
        CancellationToken cancellationToken)
    {
        var url = _configuration["Supabase:Url"]?.TrimEnd('/');
        var key = _configuration["Supabase:Key"];

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Supabase configuration is missing.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{url}/auth/v1/{endpoint}");

        request.Headers.Add("apikey", key);

        request.Content = JsonContent.Create(new
        {
            email,
            password,
            gotrue_meta_security = new
            {
                captcha_token = captchaToken
            }
        });

        using var response = await _http.SendAsync(
            request, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var document = JsonDocument.Parse(json);

        if (!response.IsSuccessStatusCode)
        {
            var message = document.RootElement.TryGetProperty("msg", out var msg)
                ? msg.GetString()
                : document.RootElement.TryGetProperty(
                    "message", out var error)
                    ? error.GetString()
                    : "Authentication failed.";

            document.Dispose();

            throw new InvalidOperationException(message);
        }

        return document;
    }
}