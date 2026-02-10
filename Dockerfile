# Sample Dockerfile for a .NET 10 Web API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file(s) and restore
COPY ["AirAware/AirAware.csproj", "AirAware/"]
RUN dotnet restore "AirAware/AirAware.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/AirAware"
RUN dotnet publish "AirAware.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AirAware.dll"]
