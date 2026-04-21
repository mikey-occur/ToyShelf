# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
	

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ToyShelf.API/ToyShelf.API.csproj", "ToyShelf.API/"]
COPY ["ToyShelf.Application/ToyShelf.Application.csproj", "ToyShelf.Application/"]
COPY ["ToyShelf.Domain/ToyShelf.Domain.csproj", "ToyShelf.Domain/"]
COPY ["ToyShelf.Infrastructure/ToyShelf.Infrastructure.csproj", "ToyShelf.Infrastructure/"]
RUN dotnet restore "./ToyShelf.API/ToyShelf.API.csproj"
COPY . .
WORKDIR "/src/ToyShelf.API"
RUN dotnet build "./ToyShelf.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ToyShelf.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# sang root cấp quyền cho thư mục wwwroot để ứng dụng có thể ghi vào đó nếu cần thiết (ví dụ: upload file, tạo log, v.v.)	
USER root
RUN mkdir -p /app/wwwroot \
    && chown -R $APP_UID:$APP_UID /app/wwwroot

# quay lại user app
USER $APP_UID

ENTRYPOINT ["dotnet", "ToyShelf.API.dll"]