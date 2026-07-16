namespace HeartJourney.Core.Services.Sanity;

public class SanityOptions
{
    public const string SectionName = "Sanity";

    public string ProjectId { get; set; } = string.Empty;

    public string Dataset { get; set; } = "production";

    public string ApiVersion { get; set; } = "v2023-10-01";

    public bool UseCdn { get; set; } = true;

    public string BaseUrl
    {
        get
        {
            var host = UseCdn ? "apicdn" : "api";
            return $"https://{ProjectId}.{host}.sanity.io/{ApiVersion}";
        }
    }
}