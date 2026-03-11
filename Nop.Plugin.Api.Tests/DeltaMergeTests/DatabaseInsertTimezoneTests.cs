using System;
using System.Collections.Generic;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Helpers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nop.Plugin.Api.Tests.DeltaMergeTests
{
    [TestFixture]
    public class DatabaseInsertTimezoneTests
    {
        private MappingHelper _mappingHelper;

        [SetUp]
        public void SetUp()
        {
            _mappingHelper = new MappingHelper();
        }

        [Test]
        [Description("Reproduce the production bug: verify DateTime.Kind before database insert")]
        public void BeforeDatabaseInsert_DateTimeShouldHaveUtcKind()
        {
            // Arrange - Simulate what the controller receives after deserialization
            var utcDateTime = new DateTime(2026, 2, 22, 15, 0, 0, DateTimeKind.Utc);

            var propertyValuePairs = new Dictionary<string, object>
            {
                { "ReadyForPickupDateUtc", utcDateTime },
                { "TrackingNumber", "TEST123" }
            };

            var newShipment = new Shipment
            {
                OrderId = 100,
                CreatedOnUtc = DateTime.UtcNow
            };

            // Act - This is what Delta.Merge does internally
            _mappingHelper.SetValues(propertyValuePairs, newShipment, typeof(Shipment), null);

            // Assert - Before saving to database via LinqToDb
            ClassicAssert.IsNotNull(newShipment.ReadyForPickupDateUtc);
            var resultDateTime = newShipment.ReadyForPickupDateUtc.Value;

            Console.WriteLine($"Input DateTime: {utcDateTime:O} (Kind: {utcDateTime.Kind})");
            Console.WriteLine($"After Merge: {resultDateTime:O} (Kind: {resultDateTime.Kind})");
            Console.WriteLine($"Server TimeZone: {TimeZoneInfo.Local.Id}");
            Console.WriteLine($"Ticks match: {utcDateTime.Ticks == resultDateTime.Ticks}");

            // The entity should have DateTimeKind.Utc BEFORE going to LinqToDb
            ClassicAssert.AreEqual(DateTimeKind.Utc, resultDateTime.Kind,
                "Entity must have Utc kind before database insert");

            ClassicAssert.AreEqual(utcDateTime.Ticks, resultDateTime.Ticks,
                "Ticks should match exactly");
        }
    }
}
