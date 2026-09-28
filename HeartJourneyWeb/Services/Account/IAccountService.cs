namespace HeartJourneyWeb.Services.Account;

public interface IAccountService
{
    Task DeleteAccountAsync(
        CancellationToken cancellationToken = default);
}