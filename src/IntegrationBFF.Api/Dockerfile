FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["IntegrationBFF.sln", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/IntegrationBFF.Core/IntegrationBFF.Core.csproj", "src/IntegrationBFF.Core/"]
COPY ["src/IntegrationBFF.Domain/IntegrationBFF.Domain.csproj", "src/IntegrationBFF.Domain/"]
COPY ["src/IntegrationBFF.Application/IntegrationBFF.Application.csproj", "src/IntegrationBFF.Application/"]
COPY ["src/IntegrationBFF.Infrastructure/IntegrationBFF.Infrastructure.csproj", "src/IntegrationBFF.Infrastructure/"]
COPY ["src/IntegrationBFF.Api/IntegrationBFF.Api.csproj", "src/IntegrationBFF.Api/"]
RUN dotnet restore src/IntegrationBFF.Api/IntegrationBFF.Api.csproj

COPY src src
RUN dotnet publish src/IntegrationBFF.Api/IntegrationBFF.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "IntegrationBFF.Api.dll"]
