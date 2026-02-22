# Date/Time Deserialization Tests

This directory contains integration tests that verify JSON date/time deserialization works correctly regardless of the server's time zone configuration.

## Purpose

These tests ensure that:
1. All DateTime values in API requests are properly interpreted as UTC
2. Date deserialization is consistent regardless of the server's local time zone
3. Various ISO 8601 date formats are handled correctly
4. Time zone offsets in date strings are properly converted to UTC

## Running the Tests

### Standard Test Run

```bash
dotnet test --filter "FullyQualifiedName~DateTimeDeserializationTests"
```

### Testing with Different Time Zones

You can manually verify the time zone independence by running tests with different TZ environment variables:

```bash
# Test with Eastern Time
TZ=America/New_York dotnet test --filter "DateTimeDeserializationTests"

# Test with Tokyo Time
TZ=Asia/Tokyo dotnet test --filter "DateTimeDeserializationTests"

# Test with London Time
TZ=Europe/London dotnet test --filter "DateTimeDeserializationTests"

# Test with UTC
TZ=UTC dotnet test --filter "DateTimeDeserializationTests"
```

All tests should pass regardless of the TZ setting, proving that date deserialization is time zone-independent.

### Running a Specific Manual Test

For detailed debugging output showing the system time zone and deserialized values:

```bash
TZ=America/New_York dotnet test --filter "ManualTest_VerifyDateDeserializationWithCustomTimeZone" --logger "console;verbosity=detailed"
```

## Test Coverage

The test suite covers:

- ✅ ISO 8601 dates without time zone markers (should be treated as UTC)
- ✅ ISO 8601 dates with explicit UTC marker (Z suffix)
- ✅ ISO 8601 dates with negative offsets (e.g., -05:00)
- ✅ ISO 8601 dates with positive offsets (e.g., +09:00)
- ✅ Date-time formats with milliseconds
- ✅ Multiple date formats in a single request
- ✅ Date-only formats (midnight UTC)
- ✅ Verification across multiple simulated time zones

## Background

This test suite was created to address issues where date handling in the API was inconsistent across different server time zones. The tests verify that the `JsonHelper.GetRequestJsonDictionaryFromStream` method correctly sets `DateTimeZoneHandling = DateTimeZoneHandling.Utc` for all JSON deserialization operations.

## Related Code

- `Nop.Plugin.Api/Helpers/JsonHelper.cs` - Sets `DateTimeZoneHandling.Utc` on JsonTextReader (line 74)
- `Nop.Plugin.Api/Converters/ApiTypeConverter.cs` - Handles date conversions for query parameters
- `Nop.Plugin.Api/DTOs/**/*Dto.cs` - DTOs with DateTime properties (e.g., OrderDto.PaidDateUtc)
