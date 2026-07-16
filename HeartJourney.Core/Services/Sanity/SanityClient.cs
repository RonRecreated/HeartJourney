using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace HeartJourney.Core.Services.Sanity;

public class SanityClient
{
    private readonly HttpClient _httpClient;
    private readonly SanityOptions _options;

    public SanityClient(HttpClient httpClient, IOptions<SanityOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<T> QueryAsync<T>(
        string groq,
        CancellationToken cancellationToken = default)
    {
        var encodedQuery = Uri.EscapeDataString(groq);

        var url =
            $"{_options.BaseUrl}/data/query/{_options.Dataset}?query={encodedQuery}";

        var response = await _httpClient.GetFromJsonAsync<SanityResponse<T>>(
            url,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException("Sanity returned no response.");
        }

        return response.Result;
    }
}