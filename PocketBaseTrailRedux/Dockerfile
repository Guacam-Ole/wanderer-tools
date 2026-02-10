FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["PocketBaseTrailRedux/PocketBaseTrailRedux.csproj", "PocketBaseTrailRedux/"]
RUN dotnet restore "PocketBaseTrailRedux/PocketBaseTrailRedux.csproj"
COPY PocketBaseTrailRedux/ PocketBaseTrailRedux/
WORKDIR "/src/PocketBaseTrailRedux"
RUN dotnet publish "PocketBaseTrailRedux.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh
ENTRYPOINT ["./entrypoint.sh"]
