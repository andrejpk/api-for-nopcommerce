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
    /// Verifies that MappingHelper.SetValues() preserves DateTimeKind.Utc when merging DTOs into entities.
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
        public void SetValues_WithUtcDateTime_PreservesDateTimeKind()
        {
            // Arrange
            var utcDateTime = new DateTime(2026, 2, 22, 22, 55, 14, DateTimeKind.Utc);
            var propertyValuePairs = new Dictionary<string, object>
            {
                { "ReadyForPickupDateUtc", utcDateTime }
            };
            var shipmentEntity = new Shipment { Id = 1, OrderId = 100 };

            // Act
            _mappingHelper.SetValues(propertyValuePairs, shipmentEntity, typeof(Shipment), null);

            // Assert
            ClassicAssert.IsNotNull(shipmentEntity.ReadyForPickupDateUtc);
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.ReadyForPickupDateUtc.Value.Kind);
            ClassicAssert.AreEqual(utcDateTime, shipmentEntity.ReadyForPickupDateUtc.Value);
        }

        [Test]
        public void SetValues_WithMultipleUtcDateTimeFields_PreservesAllDateTimeKinds()
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
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.CreatedOnUtc.Kind);
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.ReadyForPickupDateUtc.Value.Kind);
            ClassicAssert.AreEqual(DateTimeKind.Utc, shipmentEntity.ShippedDateUtc.Value.Kind);
        }

        [Test]
        public void SetValues_WithNullDateTime_SetsPropertyToNull()
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
                ReadyForPickupDateUtc = DateTime.UtcNow
            };

            // Act
            _mappingHelper.SetValues(propertyValuePairs, shipmentEntity, typeof(Shipment), null);

            // Assert
            ClassicAssert.IsNull(shipmentEntity.ReadyForPickupDateUtc);
        }
    }
}
