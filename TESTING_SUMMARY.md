# Date Deserialization Testing Summary

## Overview

Added comprehensive integration tests to verify that JSON date/time deserialization in the API works correctly regardless of the server's time zone configuration.

## Problem Statement

Previous PRs attempted to fix date handling issues, but there was no test coverage to verify that dates are correctly deserialized as UTC regardless of the server's local time zone. This could lead to subtle bugs where the API behaves differently depending on where it's deployed.

## Solution

Created a new test suite (`DateTimeDeserializationTests`) with 12 comprehensive tests that verify:

1. ✅ Dates without timezone markers are treated as UTC
2. ✅ Dates with explicit UTC marker (Z) are handled correctly
3. ✅ Dates with timezone offsets are converted to UTC
4. ✅ Multiple date formats in a single request
5. ✅ ISO 8601 formats with milliseconds
6. ✅ Consistency across different simulated server time zones

## Files Added

- `Nop.Plugin.Api.Tests/JsonDeserializationTests/DateTimeDeserializationTests.cs` - 12 comprehensive tests
- `Nop.Plugin.Api.Tests/JsonDeserializationTests/README.md` - Documentation on running tests

## Test Results

```
Total tests: 50 (38 existing + 12 new)
     Passed: 50
     Failed: 0
   Duration: ~84ms
```

## How to Verify Time Zone Independence

Run the tests with different TZ environment variables:

```bash
# Test with different time zones
TZ=America/New_York dotnet test --filter "DateTimeDeserializationTests"
TZ=Asia/Tokyo dotnet test --filter "DateTimeDeserializationTests"
TZ=Europe/London dotnet test --filter "DateTimeDeserializationTests"
TZ=UTC dotnet test --filter "DateTimeDeserializationTests"
```

All tests pass regardless of the TZ setting, proving that date deserialization is time zone-independent.

## Key Test Cases

### 1. Dates Without Time Zone Markers
```json
{
  "created_on_utc": "2024-01-15T10:30:00"
}
```
Expected: Treated as UTC, not local time

### 2. Dates With Offsets
```json
{
  "paid_date_utc": "2024-01-15T10:30:00-05:00"
}
```
Expected: Converted from EST to UTC (15:30)

### 3. Multiple Time Zone Tests
The test suite simulates multiple server time zones:
- America/New_York
- Europe/London
- Asia/Tokyo
- UTC
- Pacific/Auckland

All tests verify that dates are consistently deserialized as UTC.

## Implementation Details

The tests verify the `JsonHelper.GetRequestJsonDictionaryFromStream` method which:
- Creates a `JsonTextReader` with `DateTimeZoneHandling = DateTimeZoneHandling.Utc`
- Ensures all DateTime values in JSON are interpreted as UTC
- Properly converts offset times to UTC

This is critical for DTOs like:
- `OrderDto.PaidDateUtc`
- `OrderDto.CreatedOnUtc`
- And other DateTime fields across the API

## Next Steps

These tests should be run in CI/CD to ensure date handling remains correct across:
- Different deployment environments
- Different server configurations
- Future code changes
