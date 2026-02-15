# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY AirAware/AirAware.csproj AirAware/

# Restore dependencies
RUN dotnet restore AirAware/AirAware.csproj

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /src/AirAware
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Expose the default ASP.NET Core port
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "AirAware.dll"]
