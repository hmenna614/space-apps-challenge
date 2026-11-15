var builder = WebApplication.CreateBuilder(args);

// Enable CORS for your local HTML frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapGet("/api/spaceweather/donki-status", async (HttpClient httpClient) =>
{
    string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
    string pastDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
    string url = $"https://api.nasa.gov/DONKI/FLR?startDate={pastDate}&endDate={today}&api_key=DEMO_KEY";

    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        HttpResponseMessage response = await httpClient.GetAsync(url, cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            return Results.Ok(new { status = "fallback", solarStormActive = false, radiationLevel = "MODERATE", source = "Fallback Cache" });
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();
        bool stormActive = jsonResponse.Length > 10 && jsonResponse.Contains("flrID");

        return Results.Ok(new 
        {
            status = "success",
            source = "NASA Live DONKI API",
            solarStormActive = stormActive,
            radiationLevel = stormActive ? "HIGH (Flare Active)" : "LOW (Nominal)",
            message = stormActive ? "Solar particle event detected!" : "Conditions nominal."
        });
    }
    catch (Exception)
    {
        return Results.Ok(new 
        { 
            status = "fallback", 
            source = "Simulated Telemetry Cache",
            solarStormActive = false, 
            radiationLevel = "MODERATE",
            message = "Network offline. Using backup cache." 
        });
    }
});

app.Run("http://localhost:5000");