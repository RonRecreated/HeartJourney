using System.Net;
using System.Reflection;
using System.Text.Json;
using HeartJourneyWeb.Helpers;
using HeartJourneyWeb.Services.Auth;
using HeartJourneyWeb.Services.Checkout;
using HeartJourneyWeb.Services.Billing;
using HeartJourneyWeb.Services.JourneyAccess;
using HeartJourneyWeb.Services.Supabase;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

// PAYWALL PREVIEW: dependency-free regression checks; no real checkout or account calls.
var count = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAILED: " + name);
    count++;
}
Check(JourneyPreviewNavigation.IsPreviewPage(typeof(HeartJourneyWeb.Features.Milestone.MilestoneIntroPage)), "intro is public");
Check(JourneyPreviewNavigation.IsPreviewPage(typeof(HeartJourneyWeb.Features.Milestone.MilestoneResumePage)), "resume can dispatch previews");
foreach (var page in new[] { typeof(HeartJourneyWeb.Features.Milestone.MilestoneDetailPage), typeof(HeartJourneyWeb.Features.Reflection.DimensionReflectionPage), typeof(HeartJourneyWeb.Features.Reflection.DimensionInsightPage) })
    Check(!JourneyPreviewNavigation.IsPreviewPage(page), "paid page stays guarded: " + page.Name);
foreach (var path in new[] { "https://example.com", "//example.com", "/\\example.com", "/%2fexample.com", "/%5cexample.com", "/%252fexample.com", "/journeys%0a", "" })
    Check(JourneyPreviewNavigation.SafeReturnUrl(path) == "/journeys", "reject unsafe redirect: " + path);
var destination = JourneyPreviewNavigation.IntroUrl("relationship", "single", fourthCard: true);
Check(JourneyPreviewNavigation.SafeReturnUrl(destination) == destination, "keep selected fourth card");
Check(JourneyPreviewNavigation.SafeReturnUrl("/journeys/a%20b") == "/journeys/a%20b", "allow safe encoded paths");

var auth = DispatchProxy.Create<IAuthService, AuthProxy>();
var proxy = (AuthProxy)(object)auth;
var httpHandler = new CheckoutHandler();
var service = new CheckoutService(new HttpClient(httpHandler), auth,
    Options.Create(new SupabaseOptions { Url = "https://api.example.test", Key = "test-key" }), new TestNavigation());
var url = await service.CreateCheckoutAsync("relationship", returnPath: destination);
Check(url == "https://checkout.example.test/session", "use checkout response URL");
using (var body = JsonDocument.Parse(httpHandler.Body!))
{
    Check(body.RootElement.GetProperty("journeySlug").GetString() == "relationship", "purchase remains journey-wide");
    Check(body.RootElement.GetProperty("successUrl").GetString() == "https://app.example.test" + destination + "&checkout=success", "success restores card four");
    Check(body.RootElement.GetProperty("cancelUrl").GetString() == "https://app.example.test" + destination + "&checkout=cancel", "cancel restores card four");
}
Check(httpHandler.Authorization == "Bearer test-token", "checkout requires authenticated request");
await service.CreateCheckoutAsync("relationship", returnPath: "//evil.example");
using (var body = JsonDocument.Parse(httpHandler.Body!))
    Check(body.RootElement.GetProperty("successUrl").GetString() == "https://app.example.test/journeys?checkout=success", "unsafe checkout return falls back locally");
var billingHandler = new BillingHandler();
var billingService = new BillingService(new HttpClient(billingHandler), auth,
    Options.Create(new SupabaseOptions { Url = "https://api.example.test", Key = "test-key" }), new NoJs());
var verification = await billingService.VerifyAsync("relationship", "journey_unlock", "test-purchase-token");
Check(verification.Status == "granted", "Play verification reads backend result");
Check(billingHandler.Authorization == "Bearer test-token", "Play verification authenticates the app user");
using (var body = JsonDocument.Parse(billingHandler.Body!))
{
    Check(body.RootElement.GetProperty("journeySlug").GetString() == "relationship", "Play verification preserves journey");
    Check(body.RootElement.GetProperty("purchaseToken").GetString() == "test-purchase-token", "Play purchase token goes to backend");
    Check(!body.RootElement.TryGetProperty("userId", out _), "client cannot select a purchase owner");
}
billingHandler.Fail = true;
try { await billingService.VerifyAsync("relationship", "journey_unlock", "test-purchase-token"); throw new Exception("FAILED: billing error accepted"); }
catch (InvalidOperationException ex) { Check(!ex.Message.Contains("private-server-detail"), "billing errors hide server internals"); }

var accessHandler = new AccessHandler();
var accessService = new JourneyAccessService(new HttpClient(accessHandler), auth,
    Options.Create(new SupabaseOptions { Url = "https://api.example.test", Key = "test-key" }));
Check(await accessService.HasAccessAsync("relationship"), "verified Play entitlement unlocks its journey");
Check(!await accessService.HasAccessAsync("other-journey"), "Play entitlement does not unlock another journey");
Check((await accessService.GetUserAccessAsync()).Single().AccessType == "google_play", "home page includes Play journeys");
accessHandler.PlayStatus = HttpStatusCode.NotFound;
Check(!await accessService.HasAccessAsync("relationship"), "Stripe rollout works before Google view deployment");
accessHandler.PlayStatus = HttpStatusCode.InternalServerError;
try { await accessService.HasAccessAsync("relationship"); throw new Exception("FAILED: access lookup error granted access"); }
catch (HttpRequestException) { Check(true, "Play access errors do not grant entitlement"); }

proxy.Token = null;
var billingCalls = billingHandler.Calls;
try { await billingService.VerifyAsync("relationship", "journey_unlock", "test-purchase-token"); throw new Exception("FAILED: Play allowed without session"); }
catch (InvalidOperationException) { Check(billingHandler.Calls == billingCalls, "signed-out user cannot verify a Play purchase"); }
var previousCalls = httpHandler.Calls;
try
{
    await service.CreateCheckoutAsync("relationship", returnPath: destination);
    throw new Exception("FAILED: checkout allowed without a token");
}
catch (InvalidOperationException) { Check(httpHandler.Calls == previousCalls, "signed-out user cannot start checkout"); }
Console.WriteLine($"Passed {count} paywall preview checks.");

public class AuthProxy : DispatchProxy
{
    public string? Token { get; set; } = "test-token";
    protected override object? Invoke(MethodInfo? method, object?[]? args) =>
        method?.Name == nameof(IAuthService.GetValidAccessTokenAsync)
            ? Task.FromResult(Token) : throw new NotSupportedException(method?.Name);
}
sealed class TestNavigation : NavigationManager
{
    public TestNavigation() => Initialize("https://app.example.test/", "https://app.example.test/journeys");
    protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
}
sealed class CheckoutHandler : HttpMessageHandler
{
    public string? Body { get; private set; }
    public string? Authorization { get; private set; }
    public int Calls { get; private set; }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Body = await request.Content!.ReadAsStringAsync(cancellationToken);
        Authorization = request.Headers.Authorization?.ToString();
        return new HttpResponseMessage(HttpStatusCode.OK)
        { Content = new StringContent("{\"checkoutUrl\":\"https://checkout.example.test/session\"}") };
    }
}

sealed class NoJs : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new NotSupportedException();
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken token, object?[]? args) => throw new NotSupportedException();
}
sealed class BillingHandler : HttpMessageHandler
{
    public string? Body { get; private set; }
    public string? Authorization { get; private set; }
    public bool Fail { get; set; }
    public int Calls { get; private set; }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Body = await request.Content!.ReadAsStringAsync(cancellationToken);
        Authorization = request.Headers.Authorization?.ToString();
        return new HttpResponseMessage(Fail ? HttpStatusCode.BadGateway : HttpStatusCode.OK)
        { Content = new StringContent(Fail ? "private-server-detail" : "{\"status\":\"granted\"}") };
    }
}
sealed class AccessHandler : HttpMessageHandler
{
    public HttpStatusCode PlayStatus { get; set; } = HttpStatusCode.OK;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var play = request.RequestUri!.AbsolutePath.EndsWith("google_play_journey_access");
        var otherJourney = request.RequestUri.Query.Contains("other-journey");
        return Task.FromResult(new HttpResponseMessage(play ? PlayStatus : HttpStatusCode.OK)
        {
            Content = new StringContent(play && !otherJourney
                ? "[{\"id\":\"00000000-0000-0000-0000-000000000001\",\"journey_slug\":\"relationship\",\"access_type\":\"google_play\",\"is_active\":true}]" : "[]")
        });
    }
}
