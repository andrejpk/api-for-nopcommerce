using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Orders.GetOrderById
{
    [TestFixture]
    public class OrderApiServiceTests_GetOrderById
    {
        [Test]
        public void WhenNullIsReturnedByTheRepository_ShouldReturnNull()
        {
            int orderId = 3;
            
            // Arange
            var orderRepo = Substitute.For<IRepository<Order>>();
            orderRepo.Table.Returns((new List<Order>()).AsQueryable());
            orderRepo.GetById(orderId).Returns(null);

            // Act  
            var cut = new OrderApiService(orderRepo);
            var result = cut.GetOrderById(orderId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        [TestCase(-2)]
        [TestCase(0)]
        public void WhenNegativeOrZeroOrderIdPassed_ShouldReturnNull(int negativeOrZeroOrderId)
        {
            // Aranges
            var orderRepoStub = Substitute.For<IRepository<Order>>();

            // Act
            var cut = new OrderApiService(orderRepoStub);
            var result = cut.GetOrderById(negativeOrZeroOrderId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        public void WhenOrderIsReturnedByTheRepository_ShouldReturnTheSameOrder()
        {
            int orderId = 3;
            var order = new Order() { Id = 3 };

            // Arange
            var orderRepo = Substitute.For<IRepository<Order>>();

            var list = new List<Order>();
            list.Add(order);

            orderRepo.Table.Returns(list.AsQueryable());
            orderRepo.GetById(orderId).Returns(order);
            
            // Act
            var cut = new OrderApiService(orderRepo);
            var result = cut.GetOrderById(orderId);

            // Assert
            ClassicAssert.AreSame(order, result);
        }
    }
}
