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
    public class ProductApiServiceTests_GetProducts_DefaultParameters
    {
        [Test]
        public void WhenCalledWithDefaultParameters_GivenNoProductsExist_ShouldReturnEmptyCollection()
        {
            // Arange
            var productsRepo = Substitute.For<IRepository<Product>>();
            productsRepo.TableNoTracking.Returns(new List<Product>().AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new ProductApiService(productsRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var products = cut.GetProducts();

            // Assert
            ClassicAssert.IsEmpty(products);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenOnlyDeletedProductsExist_ShouldReturnEmptyCollection()
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

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var products = cut.GetProducts();

            // Assert
            ClassicAssert.IsEmpty(products);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeNotDeletedProductsExist_ShouldReturnThemSortedById()
        {
            var existingProducts = new List<Product>();
            existingProducts.Add(new Product() { Id = 1 });
            existingProducts.Add(new Product() { Id = 2, Deleted = true });
            existingProducts.Add(new Product() { Id = 3 });

            var expectedCollection = existingProducts.Where(x => !x.Deleted).OrderBy(x => x.Id);

            // Arange
            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(existingProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var products = cut.GetProducts();

            // Assert
            ClassicAssert.IsNotEmpty(products);
            ClassicAssert.AreEqual(expectedCollection.Count(), products.Count);
            ClassicAssert.IsTrue(products.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeProductsExist_ShouldReturnThemSortedById()
        {
            var existingProducts = new List<Product>();
            existingProducts.Add(new Product() { Id = 2, Published = false });
            existingProducts.Add(new Product() { Id = 3 });
            existingProducts.Add(new Product() { Id = 1 });

            var expectedCollection = existingProducts.Where(x => !x.Deleted).OrderBy(x => x.Id);

            // Arange
            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.TableNoTracking.Returns(existingProducts.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var products = cut.GetProducts();

            // Assert
            ClassicAssert.IsNotEmpty(products);
            ClassicAssert.AreEqual(expectedCollection.Count(), products.Count);
            ClassicAssert.IsTrue(products.Select(x => x.Id).SequenceEqual(expectedCollection.Select(x => x.Id)));
        }
    }
}