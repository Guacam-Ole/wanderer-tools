# PocketBase Trail Reader

Eine C# Konsolenanwendung zum Auslesen von Trail-Daten aus einer PocketBase-Datenbank.

## Voraussetzungen

- .NET 8.0 SDK oder höher
- Zugriff auf eine PocketBase-Instanz mit einer "trails" Collection

## Installation

1. Projekt-Verzeichnis öffnen:
   ```bash
   cd PocketBaseTrailReader
   ```

2. NuGet-Pakete wiederherstellen:
   ```bash
   dotnet restore
   ```

## Konfiguration

Bearbeite die Datei `config.json` und trage deine PocketBase-Verbindungsdaten ein:

```json
{
  "PocketBase": {
    "Url": "http://127.0.0.1:8090",
    "AdminEmail": "admin@example.com",
    "AdminPassword": "your-password-here"
  }
}
```

### Parameter:
- **Url**: Die URL deiner PocketBase-Instanz
- **AdminEmail**: Die E-Mail-Adresse des Admin-Accounts
- **AdminPassword**: Das Passwort des Admin-Accounts

## Verwendung

Programm ausführen:
```bash
dotnet run
```

## Funktionalität

Das Programm:
1. Verbindet sich mit der PocketBase-Datenbank
2. Authentifiziert sich als Admin
3. Liest alle Einträge aus der "trails" Collection
4. Gibt folgende Felder aus:
   - **id**: Die eindeutige ID des Trails
   - **name**: Der Name des Trails
   - **author**: Der Autor des Trails
   - **gpx**: Der Dateiname der GPX-Datei

## Projektstruktur

```
PocketBaseTrailReader/
├── Configuration/
│   └── PocketBaseConfig.cs    # Konfigurationsmodell
├── Models/
│   └── Trail.cs                # Trail-Datenmodell
├── Program.cs                  # Hauptprogramm
├── config.json                 # Konfigurationsdatei
└── PocketBaseTrailReader.csproj # Projektdatei
```

## Abhängigkeiten

- **pocketbase-csharp-sdk** (2.2.3): SDK für PocketBase-Verbindung
- **Microsoft.Extensions.Configuration** (8.0.0): Konfigurationsverwaltung
- **Microsoft.Extensions.Configuration.Json** (8.0.0): JSON-Konfigurationsunterstützung

## Fehlerbehandlung

Das Programm gibt detaillierte Fehlermeldungen aus, falls:
- Die Konfigurationsdatei nicht gefunden wird
- Die Verbindung zur Datenbank fehlschlägt
- Die Authentifizierung fehlschlägt
- Beim Abrufen der Daten ein Fehler auftritt
