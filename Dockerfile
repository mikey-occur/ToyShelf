# =====================================================
# ToyShelf API - Production Dockerfile for Render
# .NET 8.0 Web API with multi-stage build optimization
# =====================================================

# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY ToyShelf.sln ./
COPY ToyShelf.API/ToyShelf.API.csproj ToyShelf.API/
COPY ToyShelf.Application/ToyShelf.Application.csproj ToyShelf.Application/
COPY ToyShelf.Domain/ToyShelf.Domain.csproj ToyShelf.Domain/
COPY ToyShelf.Infrastructure/ToyShelf.Infrastructure.csproj ToyShelf.Infrastructure/

# Restore dependencies (cached layer)
RUN dotnet restore ToyShelf.sln

# Copy all source code
COPY . .

# Ensure wwwroot path exists so fallback copy step always has a source path
RUN mkdir -p /src/ToyShelf.API/wwwroot

# Build and publish in Release mode
RUN dotnet publish ToyShelf.API/ToyShelf.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Verify wwwroot is published (debug step)
RUN ls -la /app/publish/ && \
    (ls -la /app/publish/wwwroot/ 2>/dev/null || echo "wwwroot not found in publish")

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

ARG APP_PORT=5035

# Install icu-libs for globalization support, curl for health checks, and set timezone
RUN apk add --no-cache \
    icu-libs \
    curl \
    tzdata \
    && cp /usr/share/zoneinfo/Asia/Ho_Chi_Minh /etc/localtime \
    && echo "Asia/Ho_Chi_Minh" > /etc/timezone \
    && apk del tzdata

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup \
    && adduser -S appuser -u 1001 -G appgroup

WORKDIR /app

# Copy published app from build stage
COPY --from=build --chown=appuser:appgroup /app/publish .

# Explicitly copy wwwroot if it exists (fallback)
COPY --from=build --chown=appuser:appgroup /src/ToyShelf.API/wwwroot ./wwwroot/

# Switch to non-root user
USER appuser

# Expose default port (Render can override with PORT env var)
EXPOSE 5035

# Environment variables for production
ENV APP_PORT=5035 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Health check with fallback: prefer /health, fallback to root connectivity
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD sh -c "curl -fsS http://localhost:${PORT:-${APP_PORT}}/health >/dev/null || curl -sS http://localhost:${PORT:-${APP_PORT}}/ -o /dev/null || exit 1"

# Start app and bind to Render PORT if provided
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-${APP_PORT}} dotnet ToyShelf.API.dll"]