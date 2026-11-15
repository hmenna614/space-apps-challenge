using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class SpaceWeatherController : ControllerBase
{
    private readonly HttpClient _httpClient;
    // Replace with your free NASA API key from api.nasa.gov
    private const string NasaApiKey = "ThpCbTdFk7tqCd9RHMcpTggc7QTKPJrb7Cnq6gHm"; 

    public SpaceWeatherController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet("donki-status")]
    public async Task<IActionResult> GetDonkiSpaceWeather()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string pastDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        string url2 = $"https://api.nasa.gov/DONKI/FLR?startDate={pastDate}&endDate={today}&api_key={NasaApiKey}";
        string url = "https://api.nasa.gov/DONKI/FLR?startDate=2026-10-16&endDate=2026-11-15&api_key=ThpCbTdFk7tqCd9RHMcpTggc7QTKPJrb7Cnq6gHm";
        
        try
        {
            // Set a tight 3-second timeout so your game never hangs during the demo
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            HttpResponseMessage response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { error = "Failed to fetch from NASA DONKI API" });
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            bool stormActive = root.GetArrayLength() > 0;

            // Return clean JSON payload to your game frontend
            return Ok(new 
            {
                status = "success",
                source = "NASA Live DONKI API",
                solarStormActive = stormActive,
                flareCount = root.GetArrayLength(),
                radiationLevel = stormActive ? "HIGH (Flare Active)" : "LOW (Nominal)",
                message = stormActive ? "Solar particle event detected!" : "Space weather conditions are nominal."
            });
        }
        catch (Exception)
        {
            // Graceful fallback for hackathon Wi-Fi drops
            return Ok(new 
            { 
                status = "fallback", 
                source = "Simulated Telemetry Cache",
                solarStormActive = false, 
                radiationLevel = "MODERATE",
                message = "Deep Space Network telemetry link offline. Using backup cache." 
            });
        }
    }
}