using System;
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
    public class ProductApiServiceTests_GetProducts_UpdatedParameters
    {
        private IProductApiService _productApiService;
        private List<Product> _existigProducts;
        private DateTime _baseDate;

        [SetUp]
        public void Setup()
        {
            _baseDate = new DateTime(2016, 2, 12);

            _existigProducts = new List<Product>()
            {
                new Product() {Id = 2, UpdatedOnUtc = _baseDate.AddMonths(2) },
                new Product() {Id = 3, UpdatedOnUtc = _baseDate.AddMonths(6) },
                new Product() {Id = 1, UpdatedOnUtc = _baseDate.AddMonths(7) },
                new Product() {Id = 4, UpdatedOnUtc = _baseDate },
                new Product() {Id = 5, UpdatedOnUtc = _baseDate.AddMonths(3) },
                new Product() {Id = 6, Deleted = true, UpdatedOnUtc = _baseDate.AddMonths(10) },
                new Product() {Id = 7, Published = false, UpdatedOnUtc = _baseDate.AddMonths(4) }
            };

            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(_existigProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            _productApiService = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
        }

        [Test]
        public void WhenCalledWithUpdatedAtMinParameter_GivenSomeProductsUpdatedAfterThatDate_ShouldReturnThemSortedById()
        {
            // Arange
            DateTime updatedAtMinDate = _baseDate.AddMonths(5);
            var expectedCollection = _existigProducts.Where(x => x.UpdatedOnUtc > updatedAtMinDate && !x.Deleted).OrderBy(x => x.Id);
            var expectedProductsCount = expectedCollection.Count();

            // Act
            var products = _productApiService.GetProducts(updatedAtMin: updatedAtMinDate);

            // Assert
            ClassicAssert.IsNotEmpty(products);
            ClassicAssert.AreEqual(expectedProductsCount, products.Count);
            ClassicAssert.IsTrue(products.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }

        [Test]
        public void WhenCalledWithUpdatedAtMinParameter_GivenAllProductsUpdatedBeforeThatDate_ShouldReturnEmptyCollection()
        {
            // Arange
            DateTime updatedAtMinDate = _baseDate.AddMonths(7);

            // Act
            var products = _productApiService.GetProducts(updatedAtMin: updatedAtMinDate);

            // Assert
            ClassicAssert.IsEmpty(products);
        }

        [Test]
        public void WhenCalledWithUpdatedAtMaxParameter_GivenSomeProductsUpdatedBeforeThatDate_ShouldReturnThemSortedById()
        {
            // Arange
            DateTime updatedAtMaxDate = _baseDate.AddMonths(5);
            var expectedCollection =
                _existigProducts.Where(x => x.UpdatedOnUtc < updatedAtMaxDate && !x.Deleted).OrderBy(x => x.Id);

            var expectedProductsCount = expectedCollection.Count();

            // Act
            var products = _productApiService.GetProducts(updatedAtMax: updatedAtMaxDate);

            // Assert
            ClassicAssert.IsNotEmpty(products);
            ClassicAssert.AreEqual(expectedProductsCount, products.Count);
            ClassicAssert.IsTrue(products.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }

        [Test]
        public void WhenCalledWithUpdatedAtMaxParameter_GivenAllProductsUpdatedAfterThatDate_ShouldReturnEmptyCollection()
        {
            // Arange
            DateTime updatedAtMaxDate = _baseDate.Subtract(new TimeSpan(365)); // subtract one year

            // Act
            var products = _productApiService.GetProducts(updatedAtMax: updatedAtMaxDate);

            // Assert
            ClassicAssert.IsEmpty(products);
        }
    }
}