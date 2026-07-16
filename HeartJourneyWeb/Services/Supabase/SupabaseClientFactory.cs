using Microsoft.Extensions.Options;
using Supabase;
using SupabaseClientOptions = Supabase.SupabaseOptions;

namespace HeartJourneyWeb.Services.Supabase;

public class SupabaseClientFactory
{
    private readonly SupabaseOptions _options;

    public SupabaseClientFactory(IOptions<SupabaseOptions> options)
    {
        _options = options.Value;
    }

    public Client CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_options.Url))
        {
            throw new InvalidOperationException("Supabase Url is missing.");
        }

        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new InvalidOperationException("Supabase Key is missing.");
        }

        return new Client(
            _options.Url,
            _options.Key,
            new SupabaseClientOptions
            {
                AutoConnectRealtime = false
            });
    }
}