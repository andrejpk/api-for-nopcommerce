using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Categories.GetCategoriesCount
{
    using Nop.Core.Domain.Stores;
    using Nop.Services.Stores;

    [TestFixture]
    public class ProductApiServiceTests_GetProductsCount_PublishedStatusParameter
    {
        private IProductApiService _productApiService;
        private List<Product> _existigProducts;

        [SetUp]
        public void Setup()
        {
            _existigProducts = new List<Product>()
            {
                new Product() {Id = 2, Published = true },
                new Product() {Id = 3, Published = true  },
                new Product() {Id = 1, Published = false  },
                new Product() {Id = 4, Published = true  },
                new Product() {Id = 5, Published = true  },
                new Product() {Id = 6, Deleted = true, Published = true  },
                new Product() {Id = 7, Published = false }
            };

            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(_existigProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();
            
            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Product>.Is.Anything).Returns(true);

            _productApiService = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
        }

        [Test]
        public void WhenAskForOnlyThePublishedProducts_ShouldReturnOnlyThePublishedProductsCount()
        {
            // Arange
            var expectedProductsCount = _existigProducts.Count(x => x.Published && !x.Deleted);

            // Act
            var productsCount = _productApiService.GetProductsCount(publishedStatus: true);

            // Assert
            ClassicAssert.AreEqual(expectedProductsCount, productsCount);
        }

        [Test]
        public void WhenAskForOnlyTheUnpublishedProducts_ShouldReturnOnlyTheUnpublishedProductsCount()
        {
            // Arange
            var expectedCollectionCount = _existigProducts.Count(x => !x.Published && !x.Deleted);

            // Act
            var productsCount = _productApiService.GetProductsCount(publishedStatus: false);

            // Assert
            ClassicAssert.AreEqual(expectedCollectionCount, productsCount);
        }
    }
}