using System.Text.Json;
using Microsoft.JSInterop;

namespace HeartJourneyWeb.Services.BrowserStorage;

public class BrowserStorageService
{
    private readonly IJSRuntime _jsRuntime;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BrowserStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);

        await _jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            key,
            json);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            key);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task RemoveAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync(
            "localStorage.removeItem",
            key);
    }
}