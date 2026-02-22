using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Categories.GetCategoriesCount
{
    using Nop.Services.Stores;

    [TestFixture]
    public class CategoryApiServiceTests_GetCategoriesCount_PublishedStatusParameter
    {
        private ICategoryApiService _categoryApiService;
        private List<Category> _existigCategories;

        [SetUp]
        public void Setup()
        {
            _existigCategories = new List<Category>()
            {
                new Category() {Id = 2, Published = true },
                new Category() {Id = 3, Published = true  },
                new Category() {Id = 1, Published = false  },
                new Category() {Id = 4, Published = true  },
                new Category() {Id = 5, Published = true  },
                new Category() {Id = 6, Deleted = true, Published = true  },
                new Category() {Id = 7, Published = false }
            };

            var categoryRepo = Substitute.For<IRepository<Category>>();
            categoryRepo.TableNoTracking.Returns(_existigCategories.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Category>.Is.Anything).Returns(true);

            _categoryApiService = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
        }

        [Test]
        public void WhenAskForOnlyThePublishedCategories_ShouldReturnOnlyThePublishedCategoriesCount()
        {
            // Arange
            var expectedCategoriesCount = _existigCategories.Count(x => x.Published && !x.Deleted);

            // Act
            var categoriesCount = _categoryApiService.GetCategoriesCount(publishedStatus: true);

            // Assert
            ClassicAssert.AreEqual(expectedCategoriesCount, categoriesCount);
        }

        [Test]
        public void WhenAskForOnlyTheUnpublishedCategories_ShouldReturnOnlyTheUnpublishedCategoriesCount()
        {
            // Arange
            var expectedCollectionCount = _existigCategories.Count(x => !x.Published && !x.Deleted);

            // Act
            var categoriesCount = _categoryApiService.GetCategoriesCount(publishedStatus: false);

            // Assert
            ClassicAssert.AreEqual(expectedCollectionCount, categoriesCount);
        }
    }
}