# syntax=docker/dockerfile:1

# 1. Build the React SPA.
FROM node:22-alpine AS web
WORKDIR /src/safetyops-web
COPY safetyops-web/package.json safetyops-web/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY safetyops-web/ ./
RUN npm run build

# 2. Publish the API with the SPA in wwwroot, so static asset endpoints are generated for it.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY Directory.Build.props .editorconfig ./
COPY SafetyOps.Api/SafetyOps.Api.csproj SafetyOps.Api/
RUN dotnet restore SafetyOps.Api/SafetyOps.Api.csproj -p:SkipSpaBuild=true
COPY SafetyOps.Api/ SafetyOps.Api/
COPY --from=web /src/safetyops-web/dist/ SafetyOps.Api/wwwroot/
RUN dotnet publish SafetyOps.Api/SafetyOps.Api.csproj -c Release -o /app --no-restore -p:SkipSpaBuild=true

# 3. Runtime: non-root, HTTP on 8080, SQLite database on the /data volume.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN mkdir -p /data && chown "$APP_UID" /data
COPY --from=api /app .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Default="Data Source=/data/safetyops.db"
EXPOSE 8080
VOLUME /data
ENTRYPOINT ["dotnet", "SafetyOps.Api.dll"]
