# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PocketBaseTrailRedux/PocketBaseTrailRedux.csproj .
RUN dotnet restore
COPY PocketBaseTrailRedux/ .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage  
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh
ENTRYPOINT ["./entrypoint.sh"]
