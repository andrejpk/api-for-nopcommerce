using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Orders;
using static Nop.Plugin.Api.Infrastructure.Constants;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.ShoppingCartItems.GetShoppingCartItems
{
    using Nop.Core;
    using Nop.Core.Domain.Stores;

    [TestFixture]
    public class ShoppingCartItemsTests_GetShoppingCartItems_LimitParameter
    {
        private IShoppingCartItemApiService _shoppingCartItemApiService;
        private List<ShoppingCartItem> _existigShoppingCartItems;

        [SetUp]
        public void Setup()
        {
            _existigShoppingCartItems = new List<ShoppingCartItem>();

            for (int i = 1; i <= 1000; i++)
            {
                _existigShoppingCartItems.Add(new ShoppingCartItem()
                {
                    Id = i
                });
            }

            var shoppingCartItemRepo = Substitute.For<IRepository<ShoppingCartItem>>();
            shoppingCartItemRepo.TableNoTracking.Returns(_existigShoppingCartItems.AsQueryable());

            var storeContext = Substitute.For<IStoreContext>();
            storeContext.Stub(x => x.CurrentStore).Return(new Store()
            {
                Id = 0
            });

            _shoppingCartItemApiService = new ShoppingCartItemApiService(shoppingCartItemRepo, storeContext);
        }

        [Test]
        public void WhenCalledWithLimitParameter_GivenMoreShoppingCartItemsThanTheLimit_ShouldReturnCollectionWithCountEqualToTheLimit()
        {
            //Arange
            var expectedLimit = 5;

            //Act
            var shoppingCartItems = _shoppingCartItemApiService.GetShoppingCartItems(limit: expectedLimit);

            // Assert
            ClassicAssert.IsNotEmpty(shoppingCartItems);
            ClassicAssert.AreEqual(expectedLimit, shoppingCartItems.Count);
        }

        [Test]
        public void WhenCalledWithLimitParameter_GivenShoppingCartItemsBellowTheLimit_ShouldReturnCollectionWithCountEqualToPassedLimit()
        {
            //Arange
            var limit = Constants.Configurations.MaxLimit + 10;

            //Act
            var shoppingCartItems = _shoppingCartItemApiService.GetShoppingCartItems(limit: limit);

            // Assert
            ClassicAssert.IsNotEmpty(shoppingCartItems);
            ClassicAssert.AreEqual(limit, shoppingCartItems.Count);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-10)]
        public void WhenCalledWithZeroOrNegativeLimitParameter_GivenSomeProducts_ShouldReturnEmptyCollection(int limit)
        {
            //Act
            var shoppingCartItems = _shoppingCartItemApiService.GetShoppingCartItems(limit: limit);

            // Assert
            ClassicAssert.IsEmpty(shoppingCartItems);
        }
    }
}