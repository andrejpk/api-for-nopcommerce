using System;
using System.Collections.Generic;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.DTO.OrderItems;
using Nop.Plugin.Api.Helpers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nop.Plugin.Api.Tests.DeltaMergeTests
{
    /// <summary>
    /// Tests to verify that MappingHelper.SetValues() preserves DateTimeKind.Utc when merging DTOs into entities.
    /// This is critical for ensuring dates are stored correctly in the database.
    ///
    /// BUG: MappingHelper converts DateTime to string and back, which loses DateTimeKind information!
    /// </summary>
    [TestFixture]
    public class DeltaMergeDateTimeTests
    {
        private IMappingHelper _mappingHelper;

        [SetUp]
        public void SetUp()
        {
            _mappingHelper = new MappingHelper();
        }

        [Test]
        public void PROOF_OF_BUG_MappingHelperLosesDateTimeKindUtc()
        {
            // ***** This test PROVES the bug exists in MappingHelper.SetValues() *****
            // This is the root cause of dates being stored incorrectly in the database!

            // Arrange - Create a UTC DateTime (as it would be deserialized from JSON with DateTimeZoneHandling.Utc)
            var utcDateTime = new DateTime(2026, 2, 22, 22, 55, 14, DateTimeKind.Utc);

            Console.WriteLine("=== BEFORE MappingHelper.SetValues ===");
            Console.WriteLine($"Input DateTime: {utcDateTime}");
            Console.WriteLine($"Input DateTimeKind: {utcDateTime.Kind}");
            ClassicAssert.AreEqual(DateTimeKind.Utc, utcDateTime.Kind, "Sanity check: input should be UTC");

            // Create property-value pairs as they would exist after JSON deserialization
            var propertyValuePairs = new Dictionary<string, object>
            {
                { "ReadyForPickupDateUtc", utcDateTime }
            };

            // Create a Shipment entity (this represents the database entity being updated)
            var shipmentEntity = new Shipment
            {
                Id = 1,
                OrderId = 100
            };

            // Act - Call MappingHelper.SetValues (this is what Delta.Merge() calls internally)
            // This happens in ShipmentsController.UpdateShipment() at line 226: shipmentDelta.Merge(shipmentToUpdate)
            _mappingHelper.SetValues(propertyValuePairs, shipmentEntity, typeof(Shipment), null);

            // Assert - Check what happened to our DateTime
            Console.WriteLine("\n=== AFTER MappingHelper.SetValues ===");
            Console.WriteLine($"Entity DateTime: {shipmentEntity.ReadyForPickupDateUtc}");
            Console.WriteLine($"Entity DateTimeKind: {shipmentEntity.ReadyForPickupDateUtc?.Kind}");

            ClassicAssert.IsNotNull(shipmentEntity.ReadyForPickupDateUtc, "DateTime should be set on entity");

            // ***** THIS ASSERTION WILL FAIL, PROVING THE BUG *****
            if (shipmentEntity.ReadyForPickupDateUtc.Value.Kind != DateTimeKind.Utc)
            {
                Console.WriteLine("\n***** BUG CONFIRMED! *****");
                Console.WriteLine($"DateTimeKind changed from {utcDateTime.Kind} to {shipmentEntity.ReadyForPickupDateUtc.Value.Kind}");
                Console.WriteLine("\nRoot Cause:");
                Console.WriteLine("MappingHelper.ConvertAndSetValueIfValid() (line 35-47):");
                Console.WriteLine("1. Converts DateTime to string: string.Format(\"{0}\", propertyValue)");
                Console.WriteLine("2. Converts string back to DateTime: converter.ConvertFromInvariantString(...)");
                Console.WriteLine("3. This loses the DateTimeKind.Utc marker!");
                Console.WriteLine("\nImpact:");
                Console.WriteLine("- Dates are sent to API as UTC (e.g., \"2026-02-22T22:55:14Z\")");
                Console.WriteLine("- JsonHelper deserializes them correctly as DateTimeKind.Utc");
                Console.WriteLine("- But MappingHelper strips the UTC marker during Delta.Merge()");
                Console.WriteLine("- Entity Framework saves them as local time to database");
                Console.WriteLine("- When read back and serialized, they have wrong values but Z suffix!");
            }

            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.ReadyForPickupDateUtc.Value.Kind,
                "CRITICAL BUG: DateTimeKind.Utc is lost in MappingHelper.SetValues()! " +
                "See MappingHelper.cs line 35-47 (ConvertAndSetValueIfValid method).");
        }

        [Test]
        public void MultipleUtcDateTimeFields_AllLoseUtcKind()
        {
            // Arrange
            var createdOnUtc = new DateTime(2026, 2, 22, 20, 0, 0, DateTimeKind.Utc);
            var readyForPickupUtc = new DateTime(2026, 2, 22, 22, 55, 14, DateTimeKind.Utc);
            var shippedUtc = new DateTime(2026, 2, 22, 23, 0, 0, DateTimeKind.Utc);

            var propertyValuePairs = new Dictionary<string, object>
            {
                { "CreatedOnUtc", createdOnUtc },
                { "ReadyForPickupDateUtc", readyForPickupUtc },
                { "ShippedDateUtc", shippedUtc }
            };

            var shipmentEntity = new Shipment { Id = 1, OrderId = 100 };

            // Act
            _mappingHelper.SetValues(propertyValuePairs, shipmentEntity, typeof(Shipment), null);

            // Assert
            Console.WriteLine($"CreatedOnUtc Kind: {shipmentEntity.CreatedOnUtc.Kind} (should be Utc)");
            Console.WriteLine($"ReadyForPickupDateUtc Kind: {shipmentEntity.ReadyForPickupDateUtc?.Kind} (should be Utc)");
            Console.WriteLine($"ShippedDateUtc Kind: {shipmentEntity.ShippedDateUtc?.Kind} (should be Utc)");

            // All three will fail with current code
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.CreatedOnUtc.Kind,
                "CreatedOnUtc loses UTC kind");
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.ReadyForPickupDateUtc.Value.Kind,
                "ReadyForPickupDateUtc loses UTC kind");
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.ShippedDateUtc.Value.Kind,
                "ShippedDateUtc loses UTC kind");
        }

        [Test]
        public void NullableDateTime_HandledCorrectly()
        {
            // Arrange
            var propertyValuePairs = new Dictionary<string, object>
            {
                { "ReadyForPickupDateUtc", null }
            };

            var shipmentEntity = new Shipment
            {
                Id = 1,
                OrderId = 100,
                ReadyForPickupDateUtc = DateTime.UtcNow // Set a value first
            };

            // Act
            _mappingHelper.SetValues(propertyValuePairs, shipmentEntity, typeof(Shipment), null);

            // Assert - Null should be preserved (this should pass)
            ClassicAssert.IsNull(shipmentEntity.ReadyForPickupDateUtc,
                "Null DateTime should be preserved");
        }
    }
}
