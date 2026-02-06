using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PocketBaseSharp;
using PocketBaseTrailReader.Configuration;
using PocketBaseTrailReader.Models;

namespace PocketBaseTrailReader.Services;

public class TrailService : ITrailService
{
    private readonly ILogger<TrailService> _logger;
    private readonly State _state;
    private readonly PocketBaseConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public TrailService(IOptions<PocketBaseConfig> config, ILogger<TrailService> logger, State state,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _state = state;
        _config = config.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<Trail>> GetAllTrailsAsync()
    {
        _logger.LogDebug("Connecting to Database");
        var client = new PocketBase(_config.Url);
        await client.Admin.AuthenticateWithPasswordAsync(_config.AdminEmail, _config.AdminPassword);

        var trails = await client.Collection("trails").GetFullListAsync<Trail>();
        return trails.IsFailed ? throw new Exception("Failed connecting to DB") : trails.Value.ToList();
    }

    public async Task<byte[]> DownloadGpxAsync(Trail trail)
    {
        var url = $"{_config.Url.TrimEnd('/')}/api/files/trails/{trail.Id}/{trail.Gpx}";
        _logger.LogDebug("Downloading GPX from {Url}", url);

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task UploadGpxAsync(Trail trail, byte[] gpxData, string fileName)
    {
        var token = await GetAdminTokenAsync();
        var url = $"{_config.Url.TrimEnd('/')}/api/collections/trails/records/{trail.Id}";
        _logger.LogDebug("Uploading GPX to {Url}", url);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(gpxData);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/gpx+xml");
        content.Add(fileContent, "gpx", fileName);

        var response = await client.PatchAsync(url, content);
        response.EnsureSuccessStatusCode();
        _logger.LogInformation("GPX file replaced for trail {TrailId}", trail.Id);
    }

    public async Task ReduceGpx()
    {
        var allTrails = await GetAllTrailsAsync();
        if (_state.LastChecked != null) allTrails = allTrails.Where(q => q.Created >= _state.LastChecked.Value || q.Updated!=null && q.Updated>=_state.LastChecked.Value).ToList();

        foreach (var trail in allTrails)
        {
            _logger.LogInformation("Checking Trail '{Name}' f(Id:'{Id}') ", trail.Name, trail.Id);
            var gpxFIle = await DownloadGpxAsync(trail);
        }
        
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var url = $"{_config.Url.TrimEnd('/')}/api/collections/_superusers/auth-with-password";
        var client = _httpClientFactory.CreateClient();

        var payload = JsonSerializer.Serialize(new
        {
            identity = _config.AdminEmail,
            password = _config.AdminPassword
        });

        var response = await client.PostAsync(url,
            new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString()
               ?? throw new Exception("No token in auth response");
    }
}