# AirAware

**Real-time Air Quality Monitoring System**

A production-ready .NET Web API for monitoring air quality data from multiple stations, computing EPA-standard Air Quality Index (AQI) values, and providing real-time access to air quality metrics.

---

## 🎯 Overview

AirAware is a comprehensive backend system designed to collect, process, and serve air quality data from distributed monitoring stations. The system accepts readings from sensors or public feeds, computes standardized AQI values based on EPA guidelines, and provides RESTful APIs for data access.

### Key Capabilities
- ✅ Multi-station air quality monitoring
- ✅ Real-time AQI computation (PM2.5 & PM10)
- ✅ EPA-standard breakpoint calculations
- ✅ Flexible data ingestion with raw payload storage
- ✅ Geolocation support for stations
- ✅ Comprehensive test coverage (89 unit tests)
- ✅ RESTful API with full CRUD operations

---

## 🏗️ Architecture

### Tech Stack
- **.NET 10** - Latest ASP.NET Core Web API
- **Entity Framework Core 10** - ORM with SQLite (production: PostgreSQL ready)
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for tests
- **GitHub Actions** - CI/CD automation

### Project Structure
```
AirAware/
├── AirAware/                    # Main Web API project
│   ├── Controllers/             # API endpoints
│   │   ├── StationController.cs     # Station management
│   │   └── ReadingController.cs     # Reading ingestion & retrieval
│   ├── Models/                  # Domain entities
│   │   ├── Station.cs              # Monitoring station
│   │   ├── Reading.cs              # Sensor reading
│   │   └── AqiRecord.cs            # Computed AQI data
│   ├── Services/                # Business logic
│   │   ├── EpaAqiCalculator.cs     # EPA AQI computation
│   │   └── IAqiCalculator.cs       # Calculator interface
│   ├── ViewModels/              # Request/Response DTOs
│   ├── Data/                    # Database context
│   │   └── AppDbContext.cs
│   └── Migrations/              # EF Core migrations
├── AirAware.Tests/              # Comprehensive test suite
│   ├── Services/                # Service layer tests
│   ├── Controllers/             # API endpoint tests
│   └── Models/                  # Domain model tests
├── .github/workflows/           # CI/CD pipelines
└── README.md                    # This file
```

---

## 📊 Data Model

### Entities

#### **Station**
Represents an air quality monitoring station with geolocation.

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Station name |
| Latitude | double | Geographic latitude |
| Longitude | double | Geographic longitude |
| Provider | string? | Data provider name |
| Metadata | string? | JSON metadata for extensibility |
| Active | bool | Soft delete flag (default: true) |
| CreatedAt | DateTime | Creation timestamp |

#### **Reading**
Raw air quality measurements from sensors.

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| StationId | Guid | Foreign key to Station |
| Pm25 | double | PM2.5 concentration (µg/m³) |
| Pm10 | double? | PM10 concentration (µg/m³, optional) |
| RawPayload | string? | Original JSON payload from sensor |
| CreatedAt | DateTime | Reading timestamp |

#### **AqiRecord**
Computed Air Quality Index values.

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| ReadingId | Guid | Foreign key to Reading |
| StationId | Guid | Foreign key to Station |
| AqiValue | int | Overall AQI value (0-500+) |
| Category | string | EPA category (Good, Moderate, etc.) |
| Pm25Aqi | int? | PM2.5 AQI component |
| Pm10Aqi | int? | PM10 AQI component |
| Pm25Category | string? | PM2.5 category |
| Pm10Category | string? | PM10 category |
| ComputedAt | DateTime | Computation timestamp |

---

## 🔌 API Endpoints

### Station Management

#### `GET /api/v1/stations`
List all stations.

**Response:** `200 OK`
```json
[
  {
    "id": "uuid",
    "name": "Downtown Station",
    "latitude": 40.7128,
    "longitude": -74.0060,
    "provider": "PurpleAir",
    "active": true,
    "createdAt": "2026-02-08T10:00:00Z"
  }
]
```

#### `GET /api/v1/stations/{id}`
Get station by ID.

**Response:** `200 OK` or `404 Not Found`

#### `POST /api/v1/stations`
Create a new monitoring station.

**Request Body:**
```json
{
  "name": "Downtown Station",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "provider": "PurpleAir",
  "metadata": "{\"sensorType\":\"optical\"}"
}
```

**Response:** `201 Created`

#### `PUT /api/v1/stations/{id}`
Update station (partial update supported).

**Request Body:**
```json
{
  "name": "Updated Name",
  "active": false
}
```

**Response:** `200 OK` or `404 Not Found`

#### `GET /api/v1/stations/{id}/aqi/latest`
Get latest AQI data for a station.

**Response:** `200 OK`
```json
{
  "id": "uuid",
  "aqiValue": 101,
  "category": "Unhealthy for Sensitive Groups",
  "computedAt": "2026-02-08T10:15:00Z",
  "reading": {
    "id": "uuid",
    "pm25": 35.5,
    "pm10": 154,
    "createdAt": "2026-02-08T10:14:00Z"
  }
}
```

### Reading Ingestion

#### `GET /api/v1/readings`
List all readings.

**Response:** `200 OK`

#### `GET /api/v1/readings/{id}`
Get reading by ID.

**Response:** `200 OK` or `404 Not Found`

#### `POST /api/v1/readings`
Submit a new air quality reading.

**Request Body:**
```json
{
  "stationId": "uuid",
  "pm25": 35.5,
  "pm10": 154,
  "rawPayload": "{\"sensor\":\"BME680\",\"temp\":22.5}"
}
```

**Response:** `201 Created`
```json
{
  "reading": {
    "id": "uuid",
    "stationId": "uuid",
    "pm25": 35.5,
    "pm10": 154,
    "createdAt": "2026-02-08T10:14:00Z"
  },
  "aqi": {
    "id": "uuid",
    "aqiValue": 101,
    "category": "Unhealthy for Sensitive Groups",
    "pm25Aqi": 101,
    "pm10Aqi": 100,
    "computedAt": "2026-02-08T10:14:00Z"
  }
}
```

**Features:**
- ✅ Validates station exists
- ✅ Automatically computes AQI on ingestion
- ✅ Extracts PM10 from `rawPayload` if not provided
- ✅ Supports multiple JSON field names: `pm10`, `pm_10`, `pm10_atm`

---

## 📐 AQI Calculation

The system implements the official **EPA Air Quality Index** calculation using standard breakpoint tables.

### EPA Breakpoints

#### PM2.5 (µg/m³)
| Concentration Range | AQI Range | Category |
|---------------------|-----------|----------|
| 0.0 - 12.0 | 0 - 50 | Good |
| 12.1 - 35.4 | 51 - 100 | Moderate |
| 35.5 - 55.4 | 101 - 150 | Unhealthy for Sensitive Groups |
| 55.5 - 150.4 | 151 - 200 | Unhealthy |
| 150.5 - 250.4 | 201 - 300 | Very Unhealthy |
| 250.5 - 500.4 | 301 - 500 | Hazardous |

#### PM10 (µg/m³)
| Concentration Range | AQI Range | Category |
|---------------------|-----------|----------|
| 0 - 54 | 0 - 50 | Good |
| 55 - 154 | 51 - 100 | Moderate |
| 155 - 254 | 101 - 150 | Unhealthy for Sensitive Groups |
| 255 - 354 | 151 - 200 | Unhealthy |
| 355 - 424 | 201 - 300 | Very Unhealthy |
| 425 - 504 | 301 - 500 | Hazardous |

### Calculation Logic
```
AQI = ((I_hi - I_lo) / (C_hi - C_lo)) × (C - C_lo) + I_lo
```

Where:
- **C** = measured concentration
- **C_lo, C_hi** = concentration breakpoints
- **I_lo, I_hi** = index breakpoints
- **Final AQI** = max(PM2.5 AQI, PM10 AQI)

See `AirAware/Services/EpaAqiCalculator.cs` for implementation.

---

## 🧪 Testing

### Comprehensive Test Suite
**89 passing tests** covering all major components:

```
AirAware.Tests/
├── Services/EpaAqiCalculatorTests.cs      (16 tests)
│   ✅ All EPA breakpoints (PM2.5 & PM10)
│   ✅ Linear interpolation accuracy
│   ✅ Edge cases and boundary values
│   ✅ Final AQI selection logic
│
├── Controllers/StationControllerTests.cs   (9 tests)
│   ✅ CRUD operations
│   ✅ Partial updates
│   ✅ Validation handling
│
├── Controllers/ReadingControllerTests.cs   (12 tests)
│   ✅ Reading ingestion
│   ✅ AQI computation integration
│   ✅ Raw payload parsing
│   ✅ PM10 extraction variants
│
└── Models/                                 (18 tests)
    ✅ Domain model behavior
    ✅ Default values
    ✅ Relationships
```

### Run Tests
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~EpaAqiCalculatorTests"
```

### Test Results
```
Test summary: total: 89, failed: 0, succeeded: 89, skipped: 0
Build succeeded ✅
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- SQLite (included) or PostgreSQL (production)
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/AirAware.git
   cd AirAware
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run migrations**
   ```bash
   cd AirAware
   dotnet ef database update
   ```

4. **Start the application**
   ```bash
   dotnet run
   ```

   The API will be available at `http://localhost:5000`

### Development Workflow

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run the application with hot reload
dotnet watch run

# Create a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

---

## 🔄 CI/CD

### GitHub Actions Workflow
Automated PR creation from feature branches to main:
- Triggers on push to `feature/*` branches
- Creates pull request automatically
- Prevents duplicate PRs

See `.github/workflows/1-feature-to-main.yml`

### Git Configuration
Set up automatic upstream tracking:
```bash
git config --global push.autoSetupRemote true
```

---

## 📦 Database Migrations

### Current Migrations
- `20260208171606_CreateAqiRecordsService` - Initial schema with Stations, Readings, AqiRecords

### Creating New Migrations
```bash
cd AirAware
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Switching to PostgreSQL
The system uses SQLite by default but is designed for PostgreSQL in production:

1. Install package:
   ```bash
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. Update `AppDbContext.cs`:
   ```csharp
   optionsBuilder.UseNpgsql("your-connection-string");
   ```

---

## 🛠️ Configuration

### Application Settings
Edit `appsettings.json` for configuration:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Database Connection
Current: SQLite (`app.db`)
Production: PostgreSQL (update connection string in `AppDbContext.cs`)

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Submit a pull request

### Code Standards
- Follow C# naming conventions
- Write unit tests for new features
- Maintain test coverage above 80%
- Document public APIs with XML comments

---

## 📄 License

Copyright (c) 2026 João Ferreira

**Non-Commercial License** - This software is free for personal and non-commercial use.

Key restrictions:
- ❌ Commercial use is strictly prohibited
- ✅ Attribution required (credit to João Ferreira)
- ✅ Free to use, modify, and distribute for non-commercial purposes

See [LICENSE](LICENSE) for full terms.

---

## 👤 Author

**João Ferreira**
- Built with .NET 10 and ❤️
- February 2026

---

## 🗺️ Roadmap

### Future Enhancements
- [ ] Authentication & Authorization (API keys)
- [ ] Rate limiting for API endpoints
- [ ] WebSocket support for real-time updates
- [ ] Historical data aggregation
- [ ] Geographic queries (nearest stations)
- [ ] Alert system for unhealthy AQI levels
- [ ] Support for additional pollutants (O3, NO2, SO2, CO)
- [ ] Data export endpoints (CSV, JSON)
- [ ] Grafana/Prometheus monitoring
- [ ] Docker containerization
- [ ] Kubernetes deployment configs

---

## 📚 References

- [EPA Air Quality Index](https://www.airnow.gov/aqi/aqi-basics/)
- [.NET 10 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [xUnit Testing](https://xunit.net/)

---

**Last Updated:** February 8, 2026

