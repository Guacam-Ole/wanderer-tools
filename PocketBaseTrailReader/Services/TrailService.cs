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
    private readonly AppConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGpxSimplificationService _gpxSimplificationService;

    public TrailService(IOptions<AppConfig> config, ILogger<TrailService> logger, State state,
        IHttpClientFactory httpClientFactory, IGpxSimplificationService gpxSimplificationService)
    {
        _logger = logger;
        _state = state;
        _config = config.Value;
        _httpClientFactory = httpClientFactory;
        _gpxSimplificationService = gpxSimplificationService;
    }

    private async Task<IReadOnlyList<T>> GetData<T>(string list, string? filter = null)
    {
        var client = new PocketBase(_config.PocketBase.Url);
        await client.Admin.AuthenticateWithPasswordAsync(_config.PocketBase.AdminEmail,
            _config.PocketBase.AdminPassword);

        var entries = await client.Collection(list).GetFullListAsync<T>(filter: filter);
        return entries.IsFailed ? throw new Exception("Failed connecting to DB") : entries.Value.ToList();
    }


    public async Task<byte[]> DownloadGpxAsync(Trail trail)
    {
        var url = $"{_config.PocketBase.Url.TrimEnd('/')}/api/files/trails/{trail.Id}/{trail.Gpx}";
        _logger.LogDebug("Downloading GPX from {Url}", url);

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task UploadGpxAsync(Trail trail, byte[] gpxData, string fileName)
    {
        var token = await GetAdminTokenAsync();
        var url = $"{_config.PocketBase.Url.TrimEnd('/')}/api/collections/trails/records/{trail.Id}";
        _logger.LogDebug("Uploading GPX to {Url}", url);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(gpxData);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/gpx+xml");
        content.Add(fileContent, "gpx", fileName);

        var response = await client.PatchAsync(url, content);
        response.EnsureSuccessStatusCode();
        _logger.LogInformation("GPX file replaced for trail '{TrailId}'", trail.Id);
    }

    private async Task<string?> GetCommentUser()
    {
        if (_config.Comments.User == null || _config.Comments.Content == null) return null;
        var activityPubUsers = await GetData<Actor>("activitypub_actors", "isLocal=true");
        return activityPubUsers.FirstOrDefault(q => q.Name == _config.Comments.User)?.Id;
    }

    public async Task ReduceGpx()
    {
        var commentuser = await GetCommentUser();
        var allCategories = await GetData<Category>("categories");
        var allTrails = await GetData<Trail>("trails", "public=true"); // TODO: filter by date
        if (_state.LastChecked != null)
            allTrails = allTrails.Where(q =>
                    q.Created >= _state.LastChecked.Value || q.Updated != null && q.Updated >= _state.LastChecked.Value)
                .ToList();

        var backupDir = Path.Combine("backups", DateTime.Now.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(backupDir);

        var smallerDir = Path.Combine(backupDir, "smaller");
        Directory.CreateDirectory(smallerDir);

        foreach (var trail in allTrails)
        {
            if (trail.Author != "w8151hey71jgaqq") continue; // Erst mal nur meine

            var category = allCategories.FirstOrDefault(q => q.Id == trail.CategoryId);
            _logger.LogInformation("Checking Trail '{Name}' on category '{Category}' (Id:'{Id}') ", trail?.Name,
                category?.Name, trail?.Id);


            if (category == null || !_config.MinDistanceMeters.TryGetValue(category.Name, out var minDistance))
            {
                _logger.LogWarning("No MinDistanceMeters configured for category '{Category}', skipping simplification",
                    category?.Name);
                continue;
            }

            var gpxData = await DownloadGpxAsync(trail);
            if (gpxData.Length < _config.MinSizeKb)
            {
                _logger.LogInformation("Won't reduce trail '{Title}' because the file size is too small ({Size} KB)",
                    trail.Name, gpxData.Length / 1024);
                continue;
            }

            var simplified = _gpxSimplificationService.Simplify(gpxData, minDistance);
            var newSizePercent = simplified.Length * 100 / gpxData.Length;
            if (newSizePercent > _config.MinReductionPercent)
            {
                _logger.LogInformation("Won't reduce trail '{Title}' because it only would be reduced by {Percent}%",
                    trail.Name,  newSizePercent);
                continue;
            }

            var filePath = Path.Combine(backupDir, $"{trail.Name}.gpx");
            await File.WriteAllBytesAsync(filePath, gpxData);
            _logger.LogDebug("Saved GPX to {Path}", filePath);
            var smallerPath = Path.Combine(smallerDir, $"{trail.Name}.gpx");
            await File.WriteAllBytesAsync(smallerPath, simplified);
            _logger.LogInformation("Simplified GPX: {OriginalSize/1024}KB -> {SimplifiedSize/1024}KB bytes",
                gpxData.Length, simplified.Length);
            
            
        }
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var url = $"{_config.PocketBase.Url.TrimEnd('/')}/api/collections/_superusers/auth-with-password";
        var client = _httpClientFactory.CreateClient();

        var payload = JsonSerializer.Serialize(new
        {
            identity = _config.PocketBase.AdminEmail,
            password = _config.PocketBase.AdminPassword
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