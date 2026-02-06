using Microsoft.Extensions.Configuration;
using pocketbase_csharp_sdk;
using PocketBaseTrailReader.Configuration;
using PocketBaseTrailReader.Models;

namespace PocketBaseTrailReader;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Konfiguration laden
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();

            var pbConfig = configuration.GetSection("PocketBase").Get<PocketBaseConfig>();

            if (pbConfig == null)
            {
                Console.WriteLine("Fehler: Konfiguration konnte nicht geladen werden.");
                return;
            }

            Console.WriteLine($"Verbinde mit PocketBase: {pbConfig.Url}");

            // PocketBase-Client erstellen
            var client = new PocketBaseClient(pbConfig.Url);

            // Als Admin authentifizieren
            await client.Admins.AuthenticateWithPassword(pbConfig.AdminEmail, pbConfig.AdminPassword);
            Console.WriteLine("Erfolgreich als Admin angemeldet.");

            // Trails abrufen
            Console.WriteLine("\nRufe Trails ab...\n");
            var trails = await client.Collections.ListAsync<Trail>("trails");

            if (trails?.Items == null || trails.Items.Count == 0)
            {
                Console.WriteLine("Keine Trails gefunden.");
                return;
            }

            Console.WriteLine($"Anzahl gefundener Trails: {trails.Items.Count}\n");
            Console.WriteLine(new string('-', 80));

            // Trails ausgeben
            foreach (var trail in trails.Items)
            {
                Console.WriteLine(trail);
                Console.WriteLine(new string('-', 80));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler aufgetreten: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
