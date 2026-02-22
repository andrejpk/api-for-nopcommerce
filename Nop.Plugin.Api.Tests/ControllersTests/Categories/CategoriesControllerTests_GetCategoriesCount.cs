
using Nop.Plugin.Api.Controllers;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.Models.CategoriesParameters;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;
using Nop.Plugin.Api.Tests.Helpers;

namespace Nop.Plugin.Api.Tests.ControllersTests.Categories
{
    using Microsoft.AspNetCore.Mvc;

    [TestFixture]
    public class CategoriesControllerTests_GetCategoriesCount
    {
        [Test]
        public void WhenNoCategoriesExist_ShouldReturnOKResultWithCountEqualToZero()
        {
            var parameters = new CategoriesCountParametersModel();

            // arrange
            var autoMocker = new RhinoAutoMocker<CategoriesController>();
            autoMocker.Get<ICategoryApiService>().Stub(x => x.GetCategoriesCount()).IgnoreArguments().Return(0);

            //  act
            IActionResult result = autoMocker.ClassUnderTest.GetCategoriesCount(parameters);

            // assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var okObjectResult = result as OkObjectResult;

            ClassicAssert.NotNull(okObjectResult);

            ClassicAssert.IsInstanceOf<CategoriesCountRootObject>(okObjectResult.Value);

            var rootObject = okObjectResult.Value as CategoriesCountRootObject;

            ClassicAssert.NotNull(rootObject);

            ClassicAssert.AreEqual(0, rootObject.Count);
        }

        [Test]
        public void WhenSingleCategoryExists_ShouldReturnOKWithCountEqualToOne()
        {
            var parameters = new CategoriesCountParametersModel();

            // arrange
            var autoMocker = new RhinoAutoMocker<CategoriesController>();
            autoMocker.Get<ICategoryApiService>().Stub(x => x.GetCategoriesCount()).IgnoreArguments().Return(1);

            // act
            IActionResult result = autoMocker.ClassUnderTest.GetCategoriesCount(parameters);

            // assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var okObjectResult = result as OkObjectResult;

            ClassicAssert.NotNull(okObjectResult);

            ClassicAssert.IsInstanceOf<CategoriesCountRootObject>(okObjectResult.Value);

            var rootObject = okObjectResult.Value as CategoriesCountRootObject;

            ClassicAssert.NotNull(rootObject);

            ClassicAssert.AreEqual(1, rootObject.Count);
        }

        [Test]
        public void WhenCertainNumberOfCategoriesExist_ShouldReturnOKWithCountEqualToSameNumberOfCategories()
        {
            var categoriesCountParametersModel = new CategoriesCountParametersModel();
            int categoriesCount = 20;

            // arrange
            var autoMocker = new RhinoAutoMocker<CategoriesController>();
            autoMocker.Get<ICategoryApiService>().Stub(x => x.GetCategoriesCount()).IgnoreArguments().Return(categoriesCount);

            // act
            IActionResult result = autoMocker.ClassUnderTest.GetCategoriesCount(categoriesCountParametersModel);

            // assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var okObjectResult = result as OkObjectResult;

            ClassicAssert.NotNull(okObjectResult);

            ClassicAssert.IsInstanceOf<CategoriesCountRootObject>(okObjectResult.Value);

            var rootObject = okObjectResult.Value as CategoriesCountRootObject;

            ClassicAssert.NotNull(rootObject);

            ClassicAssert.AreEqual(categoriesCount, rootObject.Count);
        }
    }
}