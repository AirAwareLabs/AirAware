# AirAware

Minimal MVP — Step 1

## Goal
Build a minimal .NET backend that can accept air-quality readings (from sensors or public feeds), store them, compute a simple AQI (from PM2.5), and expose an endpoint to read the latest AQI for a station.

## Features
- Station registration (admin-only)
- Ingest single reading API (requires API key header)
- Compute a simple AQI based on PM2.5 (EPA breakpoints simplified)
- Store readings and AQI records
- Query latest AQI for a station

## Tech Stack
- .NET 10 (ASP.NET Core Web API)
- EF Core with Npgsql (+ NetTopologySuite) for PostGIS mapping
- PostgreSQL with PostGIS (local via Docker Compose)

## Data Model
- **stations**: id (uuid PK), name, location (POINT, 4326), provider, metadata json, created_at
- **readings**: id (bigserial PK), station_id (FK), timestamp (timestamptz), pm2_5 (double), pm10 (double optional), raw_payload json, created_at
- **aqi_records**: id (bigserial PK), reading_id (FK), aqi_value (int), category (string), computed_at

## API Endpoints
- `POST /api/admin/stations`: Create a station (admin, protected by ADMIN_API_KEY)
- `POST /api/ingest/reading`: Ingest a reading (protected by INGEST_API_KEY)
- `GET /api/stations/{stationId}/aqi/latest`: Returns latest AQI record for the station

## AQI Calculation
AQI is computed from PM2.5 using EPA breakpoints. See `Services/EpaAqiCalculator.cs` for implementation.

## Local Development
- Use Docker Compose for Postgres+PostGIS
- Run migrations with EF Core

## Acceptance Criteria
- Able to register a station via POST /api/admin/stations with ADMIN_API_KEY
- Able to POST a reading with valid INGEST_API_KEY and stationId and it persists in DB
- An AQI is computed and persisted for the reading
- GET /api/stations/{id}/aqi/latest returns the latest computed AQI
- Unit tests exist for the AQI computation

## How to Run
1. Start Postgres+PostGIS with Docker Compose
2. Run migrations
3. Start the ASP.NET Core Web API

## Credits
- Plan and architecture based on [AIR_AWARE_STEP1_PLAN.md]

## License
See LICENSE for details. Commercial use is prohibited.

