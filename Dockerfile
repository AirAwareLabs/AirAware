# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY ["AirAware/AirAware.csproj", "AirAware/"]

# Restore dependencies
RUN dotnet restore "AirAware/AirAware.csproj"

# Copy source files
COPY ["AirAware/", "AirAware/"]

# Build the project
WORKDIR "/src/AirAware"
RUN dotnet build "AirAware.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AirAware.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create directory for SQLite database
RUN mkdir -p /app/data

# Expose port
EXPOSE 8080

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DATABASE_PATH=/app/data/app.db

# Run the application
ENTRYPOINT ["dotnet", "AirAware.dll"]

