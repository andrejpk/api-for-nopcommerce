using System.Collections.Generic;
using System.Net;

using Nop.Core.Domain.Orders;
using static Nop.Plugin.Api.Infrastructure.Constants;
using Nop.Plugin.Api.Controllers;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.Models.ShoppingCartsParameters;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ControllersTests.ShoppingCartItems
{
    using Microsoft.AspNetCore.Mvc;
    using Nop.Plugin.Api.JSON.Serializers;
    using Nop.Plugin.Api.Tests.Helpers;

    [TestFixture]
    public class ShoppingCartItemsControllerTests_GetShoppingCartItems
    {
        [Test]
        [TestCase(Configurations.MinLimit - 1)]
        [TestCase(Configurations.MaxLimit + 1)]
        public void WhenInvalidLimitParameterPassed_ShouldReturnBadRequest(int invalidLimit)
        {
            var parameters = new ShoppingCartItemsParametersModel()
            {
                Limit = invalidLimit
            };

            //Arange
            var autoMocker = new RhinoAutoMocker<ShoppingCartItemsController>();

            autoMocker.Get<IJsonFieldsSerializer>().Stub(x => x.Serialize(Arg<ShoppingCartItemsRootObject>.Is.Anything, Arg<string>.Is.Anything))
                                                        .IgnoreArguments()
                                                        .Return(string.Empty);
            //Act
            IActionResult result = autoMocker.ClassUnderTest.GetShoppingCartItems(parameters);

            //Assert
            var statusCode = ActionResultExecutor.ExecuteResult(result);

            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        public void WhenInvalidPageParameterPassed_ShouldReturnBadRequest(int invalidPage)
        {
            var parameters = new ShoppingCartItemsParametersModel()
            {
                Page = invalidPage
            };

            //Arange
            var autoMocker = new RhinoAutoMocker<ShoppingCartItemsController>();

            autoMocker.Get<IJsonFieldsSerializer>().Stub(x => x.Serialize(Arg<ShoppingCartItemsRootObject>.Is.Anything, Arg<string>.Is.Anything))
                                                        .IgnoreArguments()
                                                        .Return(string.Empty);

            //Act
            IActionResult result = autoMocker.ClassUnderTest.GetShoppingCartItems(parameters);

            //Assert
            var statusCode = ActionResultExecutor.ExecuteResult(result);

            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

        [Test]
        public void WhenSomeValidParametersPassed_ShouldCallTheServiceWithTheSameParameters()
        {
            var parameters = new ShoppingCartItemsParametersModel();

            //Arange
            var autoMocker = new RhinoAutoMocker<ShoppingCartItemsController>();

            autoMocker.Get<IShoppingCartItemApiService>().Expect(x => x.GetShoppingCartItems(null, parameters.CreatedAtMin,
                                                                               parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                               parameters.UpdatedAtMax, parameters.Limit,
                                                                               parameters.Page)).Return(new List<ShoppingCartItem>());

            //Act
            autoMocker.ClassUnderTest.GetShoppingCartItems(parameters);

            //Assert
            autoMocker.Get<IShoppingCartItemApiService>().VerifyAllExpectations();
        }

        [Test]
        public void WhenNoShoppingCartItemsExist_ShouldCallTheSerializerWithNoShoppingCartItems()
        {
            var returnedShoppingCartItemsCollection = new List<ShoppingCartItem>();

            var parameters = new ShoppingCartItemsParametersModel();

            //Arange
            var autoMocker = new RhinoAutoMocker<ShoppingCartItemsController>();

            autoMocker.Get<IShoppingCartItemApiService>().Stub(x => x.GetShoppingCartItems()).Return(returnedShoppingCartItemsCollection).IgnoreArguments();

            //Act
            autoMocker.ClassUnderTest.GetShoppingCartItems(parameters);

            //Assert
            autoMocker.Get<IJsonFieldsSerializer>().AssertWasCalled(
                x => x.Serialize(Arg<ShoppingCartItemsRootObject>.Matches(r => r.ShoppingCartItems.Count == returnedShoppingCartItemsCollection.Count),
                Arg<string>.Is.Equal(parameters.Fields)));
        }

        [Test]
        public void WhenFieldsParametersPassed_ShouldCallTheSerializerWithTheSameFields()
        {
            var parameters = new ShoppingCartItemsParametersModel()
            {
                Fields = "id,quantity"
            };

            //Arange
            var autoMocker = new RhinoAutoMocker<ShoppingCartItemsController>();
            autoMocker.Get<IShoppingCartItemApiService>().Stub(x => x.GetShoppingCartItems()).Return(new List<ShoppingCartItem>()).IgnoreArguments();

            //Act
            autoMocker.ClassUnderTest.GetShoppingCartItems(parameters);

            //Assert
            autoMocker.Get<IJsonFieldsSerializer>().AssertWasCalled(
                x => x.Serialize(Arg<ShoppingCartItemsRootObject>.Is.Anything, Arg<string>.Is.Equal(parameters.Fields)));
        }

        [Test]
        public void WhenSomeProductsExist_ShouldCallTheSerializerWithTheseProducts()
        {
            //MappingExtensions.Maps.CreateMap<ShoppingCartItem, ShoppingCartItemDto>();

            var returnedShoppingCartItemsDtoCollection = new List<ShoppingCartItem>()
            {
                new ShoppingCartItem(),
                new ShoppingCartItem()
            };

            var parameters = new ShoppingCartItemsParametersModel();

            //Arange
            var autoMocker = new RhinoAutoMocker<ShoppingCartItemsController>();
            
            autoMocker.Get<IShoppingCartItemApiService>().Stub(x => x.GetShoppingCartItems()).Return(returnedShoppingCartItemsDtoCollection).IgnoreArguments();
            //Act
            autoMocker.ClassUnderTest.GetShoppingCartItems(parameters);

            //Assert
            autoMocker.Get<IJsonFieldsSerializer>().AssertWasCalled(
                x => x.Serialize(Arg<ShoppingCartItemsRootObject>.Matches(r => r.ShoppingCartItems.Count == returnedShoppingCartItemsDtoCollection.Count),
                Arg<string>.Is.Equal(parameters.Fields)));
        }
    }
}