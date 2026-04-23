# ── Build Stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DotNetApiTest.sln ./
COPY ApiServer/ApiServer.csproj ApiServer/
RUN dotnet restore ApiServer/ApiServer.csproj

COPY ApiServer/ ApiServer/
RUN dotnet publish ApiServer/ApiServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiServer.dll"]
