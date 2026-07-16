using Supabase;

namespace HeartJourneyWeb.Services.Auth;

public class SupabaseAuthService : IAuthService
{
    private readonly Client _supabaseClient;
    private bool _isInitialized;

    public SupabaseAuthService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
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

        _isInitialized = true;

        NotifyAuthStateChanged();
    }

    public async Task<AuthResult> SignUpAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _supabaseClient.Auth.SignUp(
                email,
                password,
                new global::Supabase.Gotrue.SignUpOptions
                {
                    RedirectTo = "http://localhost:5138/auth/confirmed"
                });

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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _supabaseClient.Auth.SignIn(email, password);

            if (session is null)
            {
                return AuthResult.Failure("Sign in failed. Please check your email and password.");
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
        await _supabaseClient.Auth.SignOut();

        NotifyAuthStateChanged();
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
}