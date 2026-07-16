using System.Text.Json.Serialization;

namespace HeartJourney.Core.Services.Sanity;

public class SanityResponse<T>
{
    [JsonPropertyName("result")]
    public T Result { get; set; } = default!;
}