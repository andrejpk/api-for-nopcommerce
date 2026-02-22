
using Nop.Plugin.Api.Controllers;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;
using Nop.Plugin.Api.Tests.Helpers;

namespace Nop.Plugin.Api.Tests.ControllersTests.Customers
{
    using Microsoft.AspNetCore.Mvc;
    using Nop.Plugin.Api.JSON.Serializers;

    [TestFixture]
    public class CustomersControllerTests_GetCustomersCount
    {
        [Test]
        public void WhenNoCustomersExist_ShouldReturnOKResultWithCountEqualToZero()
        {
            // arrange
            var autoMocker = new RhinoAutoMocker<CustomersController>();

            autoMocker.Get<IJsonFieldsSerializer>().Stub(x => x.Serialize(null, null)).Return(string.Empty);
            autoMocker.Get<ICustomerApiService>().Stub(x => x.GetCustomersCount()).Return(0);

            // act
            IActionResult result = autoMocker.ClassUnderTest.GetCustomersCount();

            // assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
            ClassicAssert.AreEqual(0, ((CustomersCountRootObject)((OkObjectResult)result).Value).Count);
        }

        [Test]
        public void WhenSingleCustomerExists_ShouldReturnOKWithCountEqualToOne()
        {
            // arrange
            var autoMocker = new RhinoAutoMocker<CustomersController>();

            autoMocker.Get<IJsonFieldsSerializer>().Stub(x => x.Serialize(null, null)).Return(string.Empty);
            autoMocker.Get<ICustomerApiService>().Stub(x => x.GetCustomersCount()).Return(1);

            // act
            IActionResult result = autoMocker.ClassUnderTest.GetCustomersCount();

            // assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
            ClassicAssert.AreEqual(1, ((CustomersCountRootObject)((OkObjectResult)result).Value).Count);
        }

        [Test]
        public void WhenCertainNumberOfCustomersExist_ShouldReturnOKWithCountEqualToSameNumberOfCustomers()
        {
            // arrange
            var autoMocker = new RhinoAutoMocker<CustomersController>();

            autoMocker.Get<ICustomerApiService>().Stub(x => x.GetCustomersCount()).Return(20000);
            autoMocker.Get<IJsonFieldsSerializer>().Stub(x => x.Serialize(null, null)).Return(string.Empty);

            // act
            IActionResult result = autoMocker.ClassUnderTest.GetCustomersCount();

            // assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);
            ClassicAssert.AreEqual(20000, ((CustomersCountRootObject)((OkObjectResult)result).Value).Count);
        }
    }
}