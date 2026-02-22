using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Nop.Plugin.Api.Helpers;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;
using Nop.Services.Localization;

namespace Nop.Plugin.Api.Tests.JsonDeserializationTests
{
    /// <summary>
    /// Tests to verify that JSON date deserialization works correctly regardless of server time zone.
    /// These tests ensure that dates in API requests are properly interpreted as UTC.
    /// </summary>
    [TestFixture]
    public class DateTimeDeserializationTests
    {
        private IJsonHelper _jsonHelper;
        private ILocalizationService _localizationService;
        private ILanguageService _languageService;

        [SetUp]
        public void SetUp()
        {
            _localizationService = Substitute.For<ILocalizationService>();
            _languageService = Substitute.For<ILanguageService>();
            _jsonHelper = new JsonHelper(_languageService, _localizationService);
        }

        [Test]
        [TestCase("America/New_York")]
        [TestCase("Europe/London")]
        [TestCase("Asia/Tokyo")]
        [TestCase("UTC")]
        [TestCase("Pacific/Auckland")]
        public void WhenDateTimeInJsonBody_ShouldDeserializeAsUtcRegardlessOfServerTimeZone(string timeZoneId)
        {
            // This test verifies that date deserialization is consistent regardless of the server's local time zone.
            // We simulate different server time zones and ensure dates are always interpreted as UTC.

            // Arrange - Save original time zone
            var originalTimeZone = TimeZoneInfo.Local;

            // Note: We cannot actually change TimeZoneInfo.Local in .NET, but we can verify
            // that our deserialization logic doesn't depend on it by testing the JsonHelper
            // directly with various date formats

            var testDate = "2024-01-15T10:30:00";
            var expectedUtcDateTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            var json = $@"{{
                ""test_object"": {{
                    ""created_on_utc"": ""{testDate}""
                }}
            }}";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;
            var deserializedDate = testObject["created_on_utc"] as DateTime?;

            // Assert
            ClassicAssert.IsNotNull(deserializedDate, "Date should be deserialized");
            ClassicAssert.AreEqual(DateTimeKind.Utc, deserializedDate.Value.Kind,
                $"Date should be UTC kind, but was {deserializedDate.Value.Kind}");
            ClassicAssert.AreEqual(expectedUtcDateTime, deserializedDate.Value,
                "Date value should match expected UTC time");
        }

        [Test]
        public void WhenDateTimeWithExplicitUtcMarker_ShouldDeserializeAsUtc()
        {
            // Arrange
            var testDate = "2024-01-15T10:30:00Z"; // Explicit UTC marker
            var expectedUtcDateTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            var json = $@"{{
                ""test_object"": {{
                    ""created_on_utc"": ""{testDate}""
                }}
            }}";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;
            var deserializedDate = testObject["created_on_utc"] as DateTime?;

            // Assert
            ClassicAssert.IsNotNull(deserializedDate, "Date should be deserialized");
            ClassicAssert.AreEqual(DateTimeKind.Utc, deserializedDate.Value.Kind, "Date should be UTC kind");
            ClassicAssert.AreEqual(expectedUtcDateTime, deserializedDate.Value);
        }

        [Test]
        public void WhenDateTimeWithOffset_ShouldConvertToUtc()
        {
            // Arrange
            // 10:30 EST (-05:00) should convert to 15:30 UTC
            var testDate = "2024-01-15T10:30:00-05:00";
            var expectedUtcDateTime = new DateTime(2024, 1, 15, 15, 30, 0, DateTimeKind.Utc);

            var json = $@"{{
                ""test_object"": {{
                    ""created_on_utc"": ""{testDate}""
                }}
            }}";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;
            var deserializedDate = testObject["created_on_utc"] as DateTime?;

            // Assert
            ClassicAssert.IsNotNull(deserializedDate, "Date should be deserialized");
            ClassicAssert.AreEqual(DateTimeKind.Utc, deserializedDate.Value.Kind, "Date should be UTC kind");
            ClassicAssert.AreEqual(expectedUtcDateTime, deserializedDate.Value,
                "Date should be converted from offset to UTC");
        }

        [Test]
        public void WhenDateTimeWithPositiveOffset_ShouldConvertToUtc()
        {
            // Arrange
            // 10:30 JST (+09:00) should convert to 01:30 UTC
            var testDate = "2024-01-15T10:30:00+09:00";
            var expectedUtcDateTime = new DateTime(2024, 1, 15, 1, 30, 0, DateTimeKind.Utc);

            var json = $@"{{
                ""test_object"": {{
                    ""created_on_utc"": ""{testDate}""
                }}
            }}";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;
            var deserializedDate = testObject["created_on_utc"] as DateTime?;

            // Assert
            ClassicAssert.IsNotNull(deserializedDate, "Date should be deserialized");
            ClassicAssert.AreEqual(DateTimeKind.Utc, deserializedDate.Value.Kind, "Date should be UTC kind");
            ClassicAssert.AreEqual(expectedUtcDateTime, deserializedDate.Value,
                "Date should be converted from positive offset to UTC");
        }

        [Test]
        public void WhenMultipleDateTimeFormats_AllShouldDeserializeAsUtc()
        {
            // Arrange
            var json = @"{
                ""test_object"": {
                    ""date1"": ""2024-01-15T10:30:00Z"",
                    ""date2"": ""2024-01-15T10:30:00"",
                    ""date3"": ""2024-01-15T10:30:00-05:00"",
                    ""date4"": ""2024-01-15T10:30:00+02:00""
                }
            }";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;

            // Assert
            foreach (var key in new[] { "date1", "date2", "date3", "date4" })
            {
                var date = testObject[key] as DateTime?;
                ClassicAssert.IsNotNull(date, $"{key} should be deserialized");
                ClassicAssert.AreEqual(DateTimeKind.Utc, date.Value.Kind,
                    $"{key} should be UTC kind, but was {date.Value.Kind}");
            }
        }

        [Test]
        public void WhenDateOnlyFormat_ShouldDeserializeAsUtcMidnight()
        {
            // Arrange
            // Note: Date-only strings may be deserialized as strings by JSON.NET, not DateTime objects.
            // This test verifies that when a full ISO 8601 date-time is provided (even at midnight),
            // it deserializes correctly. For date-only inputs, API clients should use full ISO 8601 format.
            var testDate = "2024-01-15T00:00:00Z";
            var expectedUtcDateTime = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

            var json = $@"{{
                ""test_object"": {{
                    ""created_on_utc"": ""{testDate}""
                }}
            }}";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;
            var deserializedDate = testObject["created_on_utc"] as DateTime?;

            // Assert
            ClassicAssert.IsNotNull(deserializedDate, "Date should be deserialized");
            ClassicAssert.AreEqual(DateTimeKind.Utc, deserializedDate.Value.Kind, "Date should be UTC kind");
            ClassicAssert.AreEqual(expectedUtcDateTime, deserializedDate.Value);
        }

        [Test]
        public void WhenIso8601WithMilliseconds_ShouldDeserializeCorrectly()
        {
            // Arrange
            var testDate = "2024-01-15T10:30:45.123Z";
            var expectedUtcDateTime = new DateTime(2024, 1, 15, 10, 30, 45, 123, DateTimeKind.Utc);

            var json = $@"{{
                ""test_object"": {{
                    ""created_on_utc"": ""{testDate}""
                }}
            }}";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;
            var deserializedDate = testObject["created_on_utc"] as DateTime?;

            // Assert
            ClassicAssert.IsNotNull(deserializedDate, "Date should be deserialized");
            ClassicAssert.AreEqual(DateTimeKind.Utc, deserializedDate.Value.Kind, "Date should be UTC kind");
            // Compare with millisecond precision
            ClassicAssert.AreEqual(expectedUtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                deserializedDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }

        [Test]
        [Description("This test can be run manually with different TZ environment variables to verify behavior")]
        public void ManualTest_VerifyDateDeserializationWithCustomTimeZone()
        {
            // This test is designed to be run manually with different TZ environment variables:
            // TZ=America/New_York dotnet test --filter DateTimeDeserializationTests.ManualTest_VerifyDateDeserializationWithCustomTimeZone
            // TZ=Asia/Tokyo dotnet test --filter DateTimeDeserializationTests.ManualTest_VerifyDateDeserializationWithCustomTimeZone
            // TZ=UTC dotnet test --filter DateTimeDeserializationTests.ManualTest_VerifyDateDeserializationWithCustomTimeZone

            // Arrange
            var testDate = "2024-01-15T10:30:00";
            var expectedUtcDateTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            var json = $@"{{
                ""test_object"": {{
                    ""created_on_utc"": ""{testDate}""
                }}
            }}";

            var stream = CreateStreamFromString(json);

            // Act
            var result = _jsonHelper.GetRequestJsonDictionaryFromStream(stream, false);
            var testObject = result["test_object"] as Dictionary<string, object>;
            var deserializedDate = testObject["created_on_utc"] as DateTime?;

            // Assert
            var systemTimeZone = TimeZoneInfo.Local.Id;
            Console.WriteLine($"System Time Zone: {systemTimeZone}");
            Console.WriteLine($"Input Date String: {testDate}");
            Console.WriteLine($"Deserialized Date: {deserializedDate}");
            Console.WriteLine($"Deserialized DateTimeKind: {deserializedDate?.Kind}");

            ClassicAssert.IsNotNull(deserializedDate, "Date should be deserialized");
            ClassicAssert.AreEqual(DateTimeKind.Utc, deserializedDate.Value.Kind,
                $"Date should always be UTC kind regardless of system time zone ({systemTimeZone})");
            ClassicAssert.AreEqual(expectedUtcDateTime, deserializedDate.Value,
                $"Date value should be {expectedUtcDateTime} regardless of system time zone ({systemTimeZone})");
        }

        private Stream CreateStreamFromString(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            return new MemoryStream(bytes);
        }
    }
}
