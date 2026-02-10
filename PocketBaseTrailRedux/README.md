# PocketBase Trail Redux

Automatically reduces the file size of GPX tracks stored in a [Wanderer](https://github.com/Flomp/wanderer) PocketBase instance. Runs on a schedule, backs up the originals, replaces them with simplified versions, and optionally leaves a comment on the trail.

## Setup

### 1. Create your configuration

Copy the example config and fill in your values. Also copy the "state.json" to the target directory if you are mounting it as a directory

### 2. Start the service

```bash
docker compose up -d
```

## Docker Compose

The `docker-compose.yml` contains everything needed to build and run the service:

```yaml
services:
  trail-reader:
    build: .
    environment:
      - CRON_SCHEDULE=0 3 * * *
    volumes:
      - ./config.json:/app/config.json:ro
      - ./backups:/app/backups
      #- ./state.json:/app/state.json 
    restart: unless-stopped
```


### Environment variables

| Variable | Default | Description |
|---|---|---|
| **CRON_SCHEDULE** | `0 3 * * *` | How often the service runs, in standard [cron syntax](https://crontab.guru/). The default runs once a day at 03:00. |

### Volumes

| Host path | Container path | Description |
|---|---|---|
| `./config.json` | `/app/config.json` | Your configuration file (mounted read-only). |
| `./backups` | `/app/backups` | Directory where original and simplified GPX files are stored before replacement. |
| `./state.json` | `/app/state.json` | Tracks which trails have been processed. Must exist on the host before starting (see step 2). |

## Configuration

| Setting | Description                                                                                                                                                                                                          |
|---|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **LokiUrl** | URL of a Grafana Loki instance for log shipping. Leave empty to disable.                                                                                                                                             |
| **PocketBase.Url** | URL of your PocketBase / Wanderer instance (e.g. `http://localhost:8090`).                                                                                                                                           |
| **PocketBase.AdminEmail** | Email address of a PocketBase super user account.                                                                                                                                                                    |
| **PocketBase.AdminPassword** | Password for that account.                                                                                                                                                                                           |
| **MinDistanceMeters** | Minimum distance in meters between GPS points, per trail category. Points closer together than this will be merged. Higher values = smaller files but less detail. Example: `{"Biking": 10, "Walking": 5}`           |
| **MinSizeKb** | Trails with a GPX file smaller than this (in KB) are skipped entirely.                                                                                                                                               |
| **MinRequiredSizePercent** | Only replace a GPX file if the simplified version is smaller than this percentage of the original. For example, `40` means the file must be reduced to at most 40% of its original size, otherwise it is kept as-is. |
| **Comments.User** | Name of a local user (List "activitypub_actor" with "isLocal=true") in your Wanderer instance that will be used as the comment author. Leave empty to disable comments.                           |
| **Comments.Content** | The HTML content of the comment that gets posted on a trail after its GPX file was replaced.                                                                                                                         |

## Backups

Before replacing any GPX file, the original is saved to the `backups/` directory (organized by date). The simplified version is also stored there under a `smaller/` subfolder, so you can always compare or restore.
It is still recommened to do a Database backup before starting this program

## Logs

Container logs are available via:

```bash
docker compose logs -f
```

If `LokiUrl` is configured, logs are also shipped to Grafana Loki.
