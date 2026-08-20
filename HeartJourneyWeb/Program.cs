using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HeartJourney.Core.Services.Interfaces;
using HeartJourney.Core.Services.Sanity;
using HeartJourneyWeb.Services.Supabase;
using HeartJourneyWeb.Services.Auth;
using HeartJourneyWeb.Services.DimensionProgress;
using HeartJourneyWeb.Services.Profiles;
using HeartJourneyWeb.Services.ReflectionAnswers;
using HeartJourneyWeb.Services.BrowserStorage;
using HeartJourneyWeb.Services.ActionSteps;
using HeartJourneyWeb;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.Configure<SanityOptions>(
    builder.Configuration.GetSection(SanityOptions.SectionName));
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<SanityClient>();

builder.Services.Configure<SupabaseOptions>(
    builder.Configuration.GetSection(SupabaseOptions.SectionName));

builder.Services.AddScoped<SupabaseClientFactory>();

builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<SupabaseClientFactory>();
    return factory.CreateClient();
});

builder.Services.AddScoped<IAuthService, SupabaseAuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IReflectionAnswerService, ReflectionAnswerService>();
builder.Services.AddScoped<IDimensionProgressService, DimensionProgressService>();
builder.Services.AddScoped<IUserActionStepService, UserActionStepService>();
builder.Services.AddScoped<BrowserStorageService>();

await builder.Build().RunAsync();
