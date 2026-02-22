using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Categories.GetCategories
{
    using Nop.Services.Stores;

    [TestFixture]
    public class CategoryApiServiceTests_GetCategories_LimitParameter
    {
        private ICategoryApiService _categoryApiService;
        private List<Category> _existigCategories;

        [SetUp]
        public void Setup()
        {
            _existigCategories = new List<Category>();

            for (int i = 0; i < 1000; i++)
            {
                _existigCategories.Add(new Category()
                {
                    Id = i + 1
                });
            }

            _existigCategories[5].Deleted = true;
            _existigCategories[51].Published = false;

            var categoryRepo = Substitute.For<IRepository<Category>>();
            categoryRepo.TableNoTracking.Returns(_existigCategories.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            _categoryApiService = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
        }

        [Test]
        public void WhenCalledWithLimitParameter_GivenCategoriesAboveTheLimit_ShouldReturnCollectionWithCountEqualToTheLimit()
        {
            //Arange
            var expectedLimit = 5;

            //Act
            var categories = _categoryApiService.GetCategories(limit: expectedLimit);

            // Assert
            // Not Empty assert is a good practice when you assert something about collection. Because you can get a false positive if the collection is empty.
            ClassicAssert.IsNotEmpty(categories);
            ClassicAssert.AreEqual(expectedLimit, categories.Count);
        }

        [Test]
        public void WhenCalledWithLimitParameter_GivenCategoriesBellowTheLimit_ShouldReturnCollectionWithCountEqualToTheAvailableCategories()
        {
            //Arange
            var expectedLimit = _existigCategories.Count(x => !x.Deleted);

            //Act
            var categories = _categoryApiService.GetCategories(limit: expectedLimit + 10);

            // Assert
            // Not Empty assert is a good practice when you assert something about collection. Because you can get a false positive if the collection is empty.
            ClassicAssert.IsNotEmpty(categories);
            ClassicAssert.AreEqual(expectedLimit, categories.Count);
        }

        [Test]
        public void WhenCalledWithZeroLimitParameter_GivenSomeCategories_ShouldReturnEmptyCollection()
        {
            //Arange
            var expectedLimit = 0;

            //Act
            var categories = _categoryApiService.GetCategories(limit: expectedLimit);

            // Assert
            ClassicAssert.IsEmpty(categories);
        }

        [Test]
        public void WhenCalledWithNegativeLimitParameter_GivenSomeCategories_ShouldReturnEmptyCollection()
        {
            //Arange
            var expectedLimit = -10;

            //Act
            var categories = _categoryApiService.GetCategories(limit: expectedLimit);

            // Assert
            ClassicAssert.IsEmpty(categories);
        }
    }
}