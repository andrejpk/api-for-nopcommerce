using System;
using System.Collections.Generic;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Helpers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nop.Plugin.Api.Tests.DeltaMergeTests
{
    [TestFixture]
    public class DeltaMergeTimezoneTests
    {
        private MappingHelper _mappingHelper;

        [SetUp]
        public void SetUp()
        {
            _mappingHelper = new MappingHelper();
        }

        [Test]
        [Description("Simulates the production bug: DateTime should preserve value regardless of server timezone")]
        public void WhenMergingDateTime_ShouldPreserveValueRegardlessOfServerTimezone()
        {
            // Simulate what production receives
            // JS sends: 2026-02-22T15:00:00.000Z (10am EST = 15:00 UTC)
            var inputUtcDateTime = new DateTime(2026, 2, 22, 15, 0, 0, DateTimeKind.Utc);

            var propertyValuePairs = new Dictionary<string, object>
            {
                { "ReadyForPickupDateUtc", inputUtcDateTime }
            };

            var shipmentEntity = new Shipment
            {
                Id = 1,
                OrderId = 100
            };

            // ACT
            _mappingHelper.SetValues(propertyValuePairs, shipmentEntity, typeof(Shipment), null);

            // ASSERT
            ClassicAssert.IsNotNull(shipmentEntity.ReadyForPickupDateUtc);

            var result = shipmentEntity.ReadyForPickupDateUtc.Value;

            Console.WriteLine($"Input:  {inputUtcDateTime:O} (Kind: {inputUtcDateTime.Kind})");
            Console.WriteLine($"Result: {result:O} (Kind: {result.Kind})");
            Console.WriteLine($"Server timezone: {TimeZoneInfo.Local.Id}");

            // The KIND should be preserved
            ClassicAssert.AreEqual(DateTimeKind.Utc, result.Kind,
                "DateTime Kind should be Utc after merge");

            // The VALUE should be exactly the same
            ClassicAssert.AreEqual(inputUtcDateTime, result,
                "DateTime value should be preserved exactly");

            // The TICKS should match exactly
            ClassicAssert.AreEqual(inputUtcDateTime.Ticks, result.Ticks,
                $"Ticks should match: expected {inputUtcDateTime.Ticks}, got {result.Ticks}");
        }
    }
}
