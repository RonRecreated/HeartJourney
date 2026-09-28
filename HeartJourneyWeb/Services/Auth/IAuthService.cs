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

    Task<string?> GetValidAccessTokenAsync();

    Task<bool> SendPasswordResetEmailAsync(
    string email,
    CancellationToken cancellationToken = default);

    Task<bool> UpdatePasswordAsync(
    string newPassword,
    CancellationToken cancellationToken = default);

    Task<bool> EstablishRecoverySessionAsync(
    string callbackUrl,
    CancellationToken cancellationToken = default);

    Task ClearLocalSessionAsync();
}