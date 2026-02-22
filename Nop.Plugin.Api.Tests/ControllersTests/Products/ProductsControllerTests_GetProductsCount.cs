//using System.Web.Http;
//using System.Web.Http.Results;
//using Nop.Plugin.Api.Controllers;
//using Nop.Plugin.Api.DTO.Products;
//using Nop.Plugin.Api.Models.ProductsParameters;
//using Nop.Plugin.Api.Serializers;
//using Nop.Plugin.Api.Services;
//using NUnit.Framework;
using NUnit.Framework.Legacy;
//using NSubstitute;

//namespace Nop.Plugin.Api.Tests.ControllersTests.Products
//{
//    [TestFixture]
//    public class ProductsControllerTests_GetProductsCount
//    {
//        [Test]
//        public void WhenNoProductsExist_ShouldReturnOKResultWithCountEqualToZero()
//        {
//            var parameters = new ProductsCountParametersModel();

//            // arrange
//            var productsApiServiceStub = Substitute.For<IProductApiService>();
//            productsApiServiceStub.GetProductsCount()).IgnoreArguments(.Returns(0);

//            IJsonFieldsSerializer jsonFieldsSerializer = Substitute.For<IJsonFieldsSerializer>();

//            var cut = new ProductsController(productsApiServiceStub, jsonFieldsSerializer);

//            // act
//            IActionResult result = cut.GetProductsCount(parameters);

//            // assert
//            ClassicAssert.IsInstanceOf<OkNegotiatedContentResult<ProductsCountRootObject>>(result);
//            ClassicAssert.AreEqual(0, ((OkNegotiatedContentResult<ProductsCountRootObject>)result).Content.Count);
//        }

//        [Test]
//        public void WhenSingleProductExists_ShouldReturnOKWithCountEqualToOne()
//        {
//            var parameters = new ProductsCountParametersModel();

//            // arrange
//            var productsApiServiceStub = Substitute.For<IProductApiService>();
//            productsApiServiceStub.GetProductsCount()).IgnoreArguments(.Returns(1);

//            IJsonFieldsSerializer jsonFieldsSerializer = Substitute.For<IJsonFieldsSerializer>();

//            var cut = new ProductsController(productsApiServiceStub, jsonFieldsSerializer);

//            // act
//            IActionResult result = cut.GetProductsCount(parameters);

//            // assert
//            ClassicAssert.IsInstanceOf<OkNegotiatedContentResult<ProductsCountRootObject>>(result);
//            ClassicAssert.AreEqual(1, ((OkNegotiatedContentResult<ProductsCountRootObject>)result).Content.Count);
//        }

//        [Test]
//        public void WhenCertainNumberOfProductsExist_ShouldReturnOKWithCountEqualToSameNumberOfProducts()
//        {
//            var productsCountParametersModel = new ProductsCountParametersModel();
//            int productsCount = 20;

//            // arrange
//            var productsApiServiceStub = Substitute.For<IProductApiService>();
//            productsApiServiceStub.GetProductsCount()).IgnoreArguments(.Returns(productsCount);

//            IJsonFieldsSerializer jsonFieldsSerializer = Substitute.For<IJsonFieldsSerializer>();

//            var cut = new ProductsController(productsApiServiceStub, jsonFieldsSerializer);

//            // act
//            IActionResult result = cut.GetProductsCount(productsCountParametersModel);

//            // assert
//            ClassicAssert.IsInstanceOf<OkNegotiatedContentResult<ProductsCountRootObject>>(result);
//            ClassicAssert.AreEqual(productsCount, ((OkNegotiatedContentResult<ProductsCountRootObject>)result).Content.Count);
//        }
//    }
//}