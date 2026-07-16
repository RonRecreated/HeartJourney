namespace HeartJourneyWeb.Services.Auth;

public class AuthResult
{
    public bool Succeeded { get; set; }

    public string? Message { get; set; }

    public static AuthResult Success(string? message = null)
    {
        return new AuthResult
        {
            Succeeded = true,
            Message = message
        };
    }

    public static AuthResult Failure(string message)
    {
        return new AuthResult
        {
            Succeeded = false,
            Message = message
        };
    }
}