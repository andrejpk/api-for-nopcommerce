using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Orders.GetOrders
{
    [TestFixture]
    public class OrderApiServiceTests_GetOrders_DefaultParameters
    {
        [Test]
        public void WhenCalledWithDefaultParameters_GivenNoOrdersExist_ShouldReturnEmptyCollection()
        {
            // Arange
            var ordersRepo = Substitute.For<IRepository<Order>>();
            ordersRepo.TableNoTracking.Returns(new List<Order>().AsQueryable());
            
            // Act
            var cut = new OrderApiService(ordersRepo);
            var orders = cut.GetOrders();

            // Assert
            ClassicAssert.IsEmpty(orders);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenOnlyDeletedOrdersExist_ShouldReturnEmptyCollection()
        {
            var existingOrders = new List<Order>();
            existingOrders.Add(new Order() { Id = 1, Deleted = true });
            existingOrders.Add(new Order() { Id = 2, Deleted = true });

            // Arange
            var orderRepo = Substitute.For<IRepository<Order>>();
            orderRepo.TableNoTracking.Returns(existingOrders.AsQueryable());
            
            // Act
            var cut = new OrderApiService(orderRepo);
            var orders = cut.GetOrders();

            // Assert
            ClassicAssert.IsEmpty(orders);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeNotDeletedOrdersExist_ShouldReturnThemSortedById()
        {
            var existingOrders = new List<Order>();
            existingOrders.Add(new Order() { Id = 1 });
            existingOrders.Add(new Order() { Id = 2, Deleted = true });
            existingOrders.Add(new Order() { Id = 3 });

            var expectedCollection = existingOrders.Where(x => !x.Deleted).OrderBy(x => x.Id);

            // Arange
            var orderRepo = Substitute.For<IRepository<Order>>();
            orderRepo.TableNoTracking.Returns(existingOrders.AsQueryable());
            
            // Act
            var cut = new OrderApiService(orderRepo);
            var orders = cut.GetOrders();

            // Assert
            ClassicAssert.IsNotEmpty(orders);
            ClassicAssert.AreEqual(expectedCollection.Count(), orders.Count);
            ClassicAssert.IsTrue(orders.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeOrdersExist_ShouldReturnThemSortedById()
        {
            var existingOrders = new List<Order>();
            existingOrders.Add(new Order() { Id = 2 });
            existingOrders.Add(new Order() { Id = 3 });
            existingOrders.Add(new Order() { Id = 1 });

            var expectedCollection = existingOrders.Where(x => !x.Deleted).OrderBy(x => x.Id);

            // Arange
            var orderRepo = Substitute.For<IRepository<Order>>();
            orderRepo.TableNoTracking.Returns(existingOrders.AsQueryable());
            
            // Act
            var cut = new OrderApiService(orderRepo);
            var orders = cut.GetOrders();

            // Assert
            ClassicAssert.IsNotEmpty(orders);
            ClassicAssert.AreEqual(expectedCollection.Count(), orders.Count);
            ClassicAssert.IsTrue(orders.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }
    }
}