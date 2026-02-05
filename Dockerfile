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

# Build and publish in Release mode
RUN dotnet publish ToyShelf.API/ToyShelf.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

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

# Switch to non-root user
USER appuser

# Expose port (Render uses PORT env variable)
EXPOSE 5035

# Environment variables for production
ENV ASPNETCORE_URLS=http://+:5035 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:5035/health || curl -f http://localhost:5035/swagger/index.html || exit 1

# Start the application
ENTRYPOINT ["dotnet", "ToyShelf.API.dll"]
