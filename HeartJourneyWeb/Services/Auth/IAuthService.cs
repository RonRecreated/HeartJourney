namespace HeartJourneyWeb.Services.Auth;

public interface IAuthService
{
    event Action? AuthStateChanged;

    bool IsSignedIn { get; }

    string? UserId { get; }

    string? Email { get; }

    Task InitializeAsync();

    Task<AuthResult> SignUpAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthResult> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task SignOutAsync();
}