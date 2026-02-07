#!/bin/sh
echo "${CRON_SCHEDULE:-0 3 * * *} cd /app && dotnet PocketBaseTrailReader.dll >> /proc/1/fd/1 2>&1" > /etc/crontabs/root
crond -f -l 2
