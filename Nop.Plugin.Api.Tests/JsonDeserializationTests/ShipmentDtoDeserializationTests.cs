using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.OrderItems;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nop.Plugin.Api.Tests.JsonDeserializationTests
{
    /// <summary>
    /// Test DTO without the UtcDateTimeConverter to demonstrate the bug
    /// </summary>
    public class ShipmentDtoWithoutConverter
    {
        [JsonProperty("ready_for_pickup_date_utc")]
        public DateTime? ReadyForPickupDateUtc { get; set; }
    }

    [TestFixture]
    public class ShipmentDtoDeserializationTests
    {
        [Test]
        public void WhenDeserializingShipmentWithUtcDate_ShouldPreserveUtcKind()
        {
            // This reproduces the production bug where dates are saved incorrectly
            // Arrange
            var json = @"{
                ""shipment"": {
                    ""ready_for_pickup_date_utc"": ""2026-02-22T15:00:00.000Z"",
                    ""tracking_number"": ""TEST123""
                }
            }";

            // Act - Deserialize using default settings (simulating what NopCommerce does)
            var defaultSettings = new JsonSerializerSettings(); // NO DateTimeZoneHandling.Utc
            var rootObject = JsonConvert.DeserializeObject<ShipmentRootObject>(json, defaultSettings);

            // Assert
            var shipmentDto = rootObject.Shipment;
            ClassicAssert.IsNotNull(shipmentDto.ReadyForPickupDateUtc);

            // This should pass but currently fails without the UtcDateTimeConverter
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentDto.ReadyForPickupDateUtc.Value.Kind,
                $"DateTime should have Utc kind, but has {shipmentDto.ReadyForPickupDateUtc.Value.Kind}");

            // Verify the actual time value is correct
            var expectedUtc = new DateTime(2026, 2, 22, 15, 0, 0, DateTimeKind.Utc);
            ClassicAssert.AreEqual(expectedUtc, shipmentDto.ReadyForPickupDateUtc.Value);
        }

        [Test]
        public void WhenDeserializingShipmentWithMultipleDates_AllShouldBeUtc()
        {
            // Arrange
            var json = @"{
                ""shipment"": {
                    ""ready_for_pickup_date_utc"": ""2026-02-22T15:00:00.000Z"",
                    ""shipped_date_utc"": ""2026-02-21T10:30:00Z"",
                    ""delivery_date_utc"": ""2026-02-23T14:15:00.000Z""
                }
            }";

            // Act
            var defaultSettings = new JsonSerializerSettings();
            var rootObject = JsonConvert.DeserializeObject<ShipmentRootObject>(json, defaultSettings);

            // Assert
            var shipmentDto = rootObject.Shipment;
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentDto.ReadyForPickupDateUtc.Value.Kind);
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentDto.ShippedDateUtc.Value.Kind);
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentDto.DeliveryDateUtc.Value.Kind);
        }

        [Test]
        public void BugDemo_WithoutConverter_DateTimeKindIsNotUtc()
        {
            // This demonstrates the bug that exists WITHOUT the UtcDateTimeConverter
            // Arrange
            var json = @"{""ready_for_pickup_date_utc"": ""2026-02-22T15:00:00.000Z""}";

            // Act - Deserialize WITHOUT the converter (default behavior)
            var defaultSettings = new JsonSerializerSettings();
            var dto = JsonConvert.DeserializeObject<ShipmentDtoWithoutConverter>(json, defaultSettings);

            // Assert - This shows the bug!
            ClassicAssert.IsNotNull(dto.ReadyForPickupDateUtc);

            // Without the converter, the Kind is LOCAL not UTC (even though we sent "Z")
            Console.WriteLine($"Without converter: Kind = {dto.ReadyForPickupDateUtc.Value.Kind}");
            Console.WriteLine($"Expected: {DateTimeKind.Utc}");
            Console.WriteLine($"Bug exists: {dto.ReadyForPickupDateUtc.Value.Kind != DateTimeKind.Utc}");

            // This assertion documents the bug
            ClassicAssert.AreNotEqual(DateTimeKind.Utc, dto.ReadyForPickupDateUtc.Value.Kind,
                "WITHOUT the UtcDateTimeConverter, DateTime Kind is NOT Utc (this is the bug!)");
        }

        private class ShipmentRootObject
        {
            [JsonProperty("shipment")]
            public ShipmentDto Shipment { get; set; }
        }
    }
}
