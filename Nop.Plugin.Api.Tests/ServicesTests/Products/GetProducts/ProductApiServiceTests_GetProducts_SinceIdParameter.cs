using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Products.GetProducts
{
    using Nop.Services.Stores;

    [TestFixture]
    public class ProductApiServiceTests_GetProducts_SinceIdParameter
    {
        private IProductApiService _productApiService;
        private List<Product> _existigProducts;

        [SetUp]
        public void Setup()
        {
            _existigProducts = new List<Product>()
            {
                new Product() {Id = 2 },
                new Product() {Id = 3 },
                new Product() {Id = 1 },
                new Product() {Id = 4 },
                new Product() {Id = 5 },
                new Product() {Id = 6, Deleted = true },
                new Product() {Id = 7 }
            };

            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(_existigProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            _productApiService = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
        }

        [Test]
        public void WhenCalledWithValidSinceId_ShouldReturnOnlyTheProductsAfterThisIdSortedById()
        {
            // Arange
            int sinceId = 3;
            var expectedCollection = _existigProducts.Where(x => x.Id > sinceId && !x.Deleted).OrderBy(x => x.Id);

            // Act
            var products = _productApiService.GetProducts(sinceId: sinceId);

            // Assert
            ClassicAssert.IsNotEmpty(products);
            ClassicAssert.IsTrue(products.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }

        [Test]
        [TestCase(0)]
        [TestCase(-10)]
        public void WhenCalledZeroOrNegativeSinceId_ShouldReturnAllTheProductsSortedById(int sinceId)
        {
            // Arange
            var expectedCollection = _existigProducts.Where(x => x.Id > sinceId && !x.Deleted).OrderBy(x => x.Id);

            // Act
            var products = _productApiService.GetProducts(sinceId: sinceId);

            // Assert
            ClassicAssert.IsNotEmpty(products);
            ClassicAssert.IsTrue(products.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }

        [Test]
        public void WhenCalledSinceIdOutsideOfTheProductsIdsRange_ShouldReturnEmptyCollection()
        {
            // Arange
            int sinceId = int.MaxValue;
        
            // Act
            var products = _productApiService.GetProducts(sinceId: sinceId);

            // Assert
            ClassicAssert.IsEmpty(products);
        }
    }
}