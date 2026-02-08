# AirAware Unit Tests

## Overview
A comprehensive test suite for the AirAware application with **89 passing tests** covering all major components.

## Test Structure

```
AirAware.Tests/
├── Services/
│   └── AqiCalculatorTests.cs (16 tests)
├── Controllers/
│   ├── StationControllerTests.cs (9 tests)
│   └── ReadingControllerTests.cs (12 tests)
└── Models/
    ├── StationTests.cs (5 tests)
    ├── ReadingTests.cs (7 tests)
    └── AqiRecordTests.cs (6 tests)
```

## Test Coverage

### 1. **AqiCalculatorTests** (16 tests)
Tests the AQI (Air Quality Index) calculation service:
- ✅ PM2.5 AQI calculations for all EPA breakpoints
- ✅ PM10 AQI calculations for all EPA breakpoints
- ✅ Combined reading calculations (both pollutants)
- ✅ Handling null PM10 values
- ✅ Correct final AQI selection (highest pollutant)
- ✅ Linear interpolation accuracy
- ✅ Capping values above maximum breakpoints

### 2. **StationControllerTests** (9 tests)
Tests station management endpoints:
- ✅ GET all stations
- ✅ GET station by ID (existing and non-existing)
- ✅ POST create new station
- ✅ POST with metadata
- ✅ PUT update existing station (full and partial updates)
- ✅ PUT non-existing station (returns 404)
- ✅ Timestamp creation verification

### 3. **ReadingControllerTests** (12 tests)
Tests reading management and AQI record creation:
- ✅ GET all readings
- ✅ GET reading by ID (existing and non-existing)
- ✅ POST create reading with AQI calculation
- ✅ POST with non-existing station (validation)
- ✅ POST with raw payload storage
- ✅ POST with PM10 extraction from raw payload
- ✅ Alternative PM10 field names (pm_10, pm10_atm)
- ✅ Timestamp creation
- ✅ Duplicate AQI record prevention

### 4. **StationTests** (5 tests)
Tests Station model behavior:
- ✅ Default values (ID, Active, CreatedAt)
- ✅ Storage of all fields
- ✅ Valid coordinate ranges
- ✅ Unique ID generation

### 5. **ReadingTests** (7 tests)
Tests Reading model behavior:
- ✅ Default values
- ✅ All fields storage
- ✅ Nullable PM10 handling
- ✅ Nullable RawPayload handling
- ✅ PM2.5 and PM10 value ranges
- ✅ Unique ID generation

### 6. **AqiRecordTests** (6 tests)
Tests AQI Record model behavior:
- ✅ Default values (ID, ComputedAt)
- ✅ All fields storage
- ✅ Nullable PM25Aqi and Pm10Aqi
- ✅ AQI values and categories across all ranges
- ✅ Unique ID generation
- ✅ Reading and Station relationship tracking

## Technologies Used
- **xUnit**: Test framework
- **Moq**: Mocking library for dependency injection
- **EntityFrameworkCore.InMemory**: In-memory database for isolated tests

## Running Tests

### Run all tests:
```bash
dotnet test
```

### Run with detailed output:
```bash
dotnet test --verbosity normal
```

### Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~AqiCalculatorTests"
```

### Run tests with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## Test Results

```
Test summary: total: 89, failed: 0, succeeded: 89, skipped: 0
Build succeeded with warnings
```

## Key Testing Patterns

### 1. **Isolated Tests**
Each test class uses an in-memory database with a unique name to ensure complete isolation:
```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

### 2. **Arrange-Act-Assert Pattern**
All tests follow the AAA pattern for clarity:
```csharp
// Arrange
var station = new Station { ... };

// Act
var result = await _controller.GetAsync(_context);

// Assert
Assert.IsType<OkObjectResult>(result);
```

### 3. **Theory-Based Tests**
Using `[Theory]` and `[InlineData]` for testing multiple scenarios:
```csharp
[Theory]
[InlineData(0.0, 0, "Good")]
[InlineData(12.0, 50, "Good")]
[InlineData(12.1, 51, "Moderate")]
public void CalculateForPm25_ValidConcentration_ReturnsCorrectAqi(...)
```

### 4. **Mocking Dependencies**
Using Moq to mock the AQI calculator in controller tests:
```csharp
_mockCalculator
    .Setup(c => c.Calculate(It.IsAny<Reading>()))
    .Returns((finalResult, pm25Result, pm10Result));
```

### 5. **Proper Cleanup**
All test classes implement `IDisposable` to clean up database resources:
```csharp
public void Dispose()
{
    _context.Database.EnsureDeleted();
    _context.Dispose();
}
```

## Benefits

✅ **Confidence in Code Changes**: All changes are validated by automated tests  
✅ **Documentation**: Tests serve as executable documentation  
✅ **Regression Prevention**: Catch bugs before deployment  
✅ **Design Feedback**: Well-tested code tends to be better designed  
✅ **Refactoring Safety**: Safely refactor knowing tests will catch issues  

## Next Steps

Consider adding:
- Integration tests for end-to-end scenarios
- Performance tests for AQI calculations
- API contract tests
- Test coverage reporting
- CI/CD pipeline integration

