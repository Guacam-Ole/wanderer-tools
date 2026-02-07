using System.Reflection;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PocketBaseTrailReader;
using PocketBaseTrailReader.Configuration;
using PocketBaseTrailReader.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

const string stateFile = "state.json";
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);

builder.Services.Configure<AppConfig>(builder.Configuration);

builder.Services.AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Debug));
builder.Services.AddSerilog(cfg =>
{
    cfg.MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("job", Assembly.GetEntryAssembly()?.GetName().Name)
        .Enrich.WithProperty("desktop", Environment.GetEnvironmentVariable("DESKTOP_SESSION"))
        .Enrich.WithProperty("language", Environment.GetEnvironmentVariable("LANGUAGE"))
        .Enrich.WithProperty("lc", Environment.GetEnvironmentVariable("LC_NAME"))
        .Enrich.WithProperty("timezone", Environment.GetEnvironmentVariable("TZ"))
        .Enrich.WithProperty("dotnetVersion", Environment.GetEnvironmentVariable("DOTNET_VERSION"))
        .Enrich.WithProperty("inContainer", Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));

    var lokiUrl = Environment.GetEnvironmentVariable("LOKIURL") ?? builder.Configuration.Get<AppConfig>()?.LokiUrl;
    if (!string.IsNullOrEmpty(lokiUrl))
        cfg.WriteTo.GrafanaLoki(lokiUrl, propertiesAsLabels: ["job"]);

    cfg.WriteTo.Console();
});
builder.Services.AddHttpClient();
builder.Services.AddTransient<IGpxSimplificationService, GpxSimplificationService>();
builder.Services.AddTransient<ITrailService, TrailService>();
var state = new State();
if (File.Exists(stateFile))
{
    var stateConfig = File.ReadAllText(stateFile);
    state = JsonSerializer.Deserialize<State>(stateConfig);
}

builder.Services.AddSingleton<State>(state); 
var host = builder.Build();

var trailService = host.Services.GetRequiredService<ITrailService>();
await trailService.ReduceGpx();

File.WriteAllText(stateFile, JsonSerializer.Serialize(state));