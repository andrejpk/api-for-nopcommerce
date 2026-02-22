using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Products.GetProductsCount
{
    using Nop.Services.Stores;

    [TestFixture]
    public class ProductApiServiceTests_GetProductsCount_DefaultParameters
    {
        [Test]
        public void WhenCalledWithDefaultParameters_GivenNoProductsExist_ShouldReturnZero()
        {
            // Arange
            var productsRepo = Substitute.For<IRepository<Product>>();
            productsRepo.TableNoTracking.Returns(new List<Product>().AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Product>.Is.Anything).Returns(true);

            // Act
            var cut = new ProductApiService(productsRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var productsCount = cut.GetProductsCount();

            // Assert
            ClassicAssert.AreEqual(0, productsCount);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenOnlyDeletedProductsExist_ShouldReturnZero()
        {
            var existingProducts = new List<Product>();
            existingProducts.Add(new Product() { Id = 1, Deleted = true });
            existingProducts.Add(new Product() { Id = 2, Deleted = true });

            // Arange
            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(existingProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Product>.Is.Anything).Returns(true);

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var countResult = cut.GetProductsCount();

            // Assert
            ClassicAssert.AreEqual(0, countResult);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeNotDeletedProductsExist_ShouldReturnTheirCount()
        {
            var existingProducts = new List<Product>();
            existingProducts.Add(new Product() { Id = 1 });
            existingProducts.Add(new Product() { Id = 2, Deleted = true });
            existingProducts.Add(new Product() { Id = 3 });

            // Arange
            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(existingProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Product>.Is.Anything).Returns(true);

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var countResult = cut.GetProductsCount();

            // Assert
            ClassicAssert.AreEqual(2, countResult);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeProductsExist_ShouldReturnTheirCount()
        {
            var existingProducts = new List<Product>();
            existingProducts.Add(new Product() { Id = 2, Published = false });
            existingProducts.Add(new Product() { Id = 3 });
            existingProducts.Add(new Product() { Id = 1 });

            // Arange
            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(existingProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Product>.Is.Anything).Returns(true);

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var countResult = cut.GetProductsCount();

            // Assert
            ClassicAssert.AreEqual(existingProducts.Count, countResult);
        }
    }
}