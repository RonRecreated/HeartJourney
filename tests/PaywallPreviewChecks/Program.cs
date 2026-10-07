using System.Net;
using System.Reflection;
using System.Text.Json;
using HeartJourneyWeb.Helpers;
using HeartJourneyWeb.Services.Auth;
using HeartJourneyWeb.Services.Checkout;
using HeartJourneyWeb.Services.Supabase;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

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
proxy.Token = null;
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
