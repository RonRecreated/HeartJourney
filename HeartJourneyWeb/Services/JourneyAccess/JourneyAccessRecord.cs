using System.Text.Json.Serialization;

namespace HeartJourneyWeb.Services.JourneyAccess;

public class JourneyAccessRecord
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("journey_slug")]
    public string? JourneySlug { get; set; }

    [JsonPropertyName("access_type")]
    public string? AccessType { get; set; }

    [JsonPropertyName("stripe_checkout_session_id")]
    public string? StripeCheckoutSessionId { get; set; }

    [JsonPropertyName("stripe_payment_intent_id")]
    public string? StripePaymentIntentId { get; set; }

    [JsonPropertyName("granted_at")]
    public DateTimeOffset GrantedAt { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("keep_access_after_refund")]
    public bool KeepAccessAfterRefund { get; set; }

    [JsonPropertyName("refund_status")]
    public string? RefundStatus { get; set; }

    [JsonPropertyName("refunded_at")]
    public DateTimeOffset? RefundedAt { get; set; }
}