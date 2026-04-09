# ── Build Stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore（只還原 ApiServer，不包含 Tests）
COPY DotNetApiTest.sln ./
COPY ApiServer/ApiServer.csproj ApiServer/
RUN dotnet restore ApiServer/ApiServer.csproj

# 複製原始碼並發佈
COPY ApiServer/ ApiServer/
RUN dotnet publish ApiServer/ApiServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# 在容器內監聽 HTTP 8080（Nginx 負責對外）
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiServer.dll"]
