# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ENV HOME=/root \
    DOTNET_CLI_HOME=/root \
    NUGET_PACKAGES=/root/.nuget/packages

# Clean NuGet.Config (no Visual Studio fallback)
COPY NuGet.Config ./

# Restore cache per .csproj to improve Docker layer caching
COPY ["FleetRent.sln", "./"]
COPY ["FleetRent.API/FleetRent.API.csproj", "FleetRent.API/"]
COPY ["FleetRent.Application/FleetRent.Application.csproj", "FleetRent.Application/"]
COPY ["FleetRent.Domain/FleetRent.Domain.csproj", "FleetRent.Domain/"]
COPY ["FleetRent.Infrastructure/FleetRent.Infrastructure.csproj", "FleetRent.Infrastructure/"]
# Include test project in restore cache
COPY ["FleetRent.Tests/FleetRent.Tests.csproj", "FleetRent.Tests/"]

# First restore: warms up NuGet cache based on the solution and known .csproj files
RUN dotnet restore "FleetRent.sln" --configfile ./NuGet.Config --force-evaluate

# Copy the remaining source code (bin/obj excluded by .dockerignore)
COPY . .

# Second restore: ensures all packages (including analyzers) are restored
# taking into account props/targets that only exist after copying the full source tree
RUN dotnet restore "FleetRent.sln" --configfile ./NuGet.Config --force-evaluate

# Build the full solution (API + libraries + tests)
RUN dotnet build "FleetRent.sln" -c Release --no-restore

# Publish API project only
WORKDIR /src/FleetRent.API
RUN dotnet publish "FleetRent.API.csproj" -c Release -o /app/publish --no-build -v q

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FleetRent.API.dll"]
