using System;
using Nop.Plugin.Api.Converters;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nop.Plugin.Api.Tests.ConvertersTests.ApiTypeConverter
{
    [TestFixture]
    public class ApiTypeConverterTests_DateTimeKindBug
    {
        private IApiTypeConverter _apiTypeConverter;

        [SetUp]
        public void SetUp()
        {
            _apiTypeConverter = new Converters.ApiTypeConverter();
        }

        [Test]
        public void Bug_DateWithoutTimezone_ReturnsUnspecifiedInsteadOfUtc()
        {
            // Arrange
            var dateWithoutTimezone = "2024-01-15T10:30:00";

            // Act
            DateTime? result = _apiTypeConverter.ToUtcDateTimeNullable(dateWithoutTimezone);

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(DateTimeKind.Utc, result.Value.Kind);
        }

        [Test]
        public void Control_DateWithZMarker_CorrectlyReturnsUtc()
        {
            // Arrange
            var dateWithZulu = "2024-01-15T10:30:00Z";

            // Act
            DateTime? result = _apiTypeConverter.ToUtcDateTimeNullable(dateWithZulu);

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(DateTimeKind.Utc, result.Value.Kind);
        }
    }
}
