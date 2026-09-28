using HeartJourneyWeb.Services.Auth;
using Supabase;

namespace HeartJourneyWeb.Services.Account;

public class AccountService : IAccountService
{
    private readonly Client _supabaseClient;
    private readonly IAuthService _authService;

    public AccountService(
        Client supabaseClient,
        IAuthService authService)
    {
        _supabaseClient = supabaseClient;
        _authService = authService;
    }

    public async Task DeleteAccountAsync(
        CancellationToken cancellationToken = default)
    {
        var accessToken =
            await _authService.GetValidAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "Your session has expired. Please sign in again.");
        }

        var options =
            new global::Supabase.Functions.Client.InvokeFunctionOptions
            {
                Headers =
                    new Dictionary<string, string>
                    {
                        {
                            "Authorization",
                            $"Bearer {accessToken}"
                        }
                    }
            };

        await _supabaseClient.Functions.Invoke(
            "delete-account",
            options: options);
    }
}