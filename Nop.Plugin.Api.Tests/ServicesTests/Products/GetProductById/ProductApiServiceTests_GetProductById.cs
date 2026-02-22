using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Products.GetProductById
{
    using Nop.Services.Stores;

    [TestFixture]
    public class ProductApiServiceTests_GetProductById
    {
        [Test]
        public void WhenNullIsReturnedByTheRepository_ShouldReturnNull()
        {
            int productId = 3;

            // Arange
            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.GetById(productId).Returns(null);
            productRepo.Table.Returns((new List<Product>()).AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var result = cut.GetProductById(productId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        [TestCase(-2)]
        [TestCase(0)]
        public void WhenNegativeOrZeroProductIdPassed_ShouldReturnNull(int negativeOrZeroProductId)
        {
            // Aranges
            var productRepoStub = Substitute.For<IRepository<Product>>();
            productRepoStub.Table.Returns((new List<Product>()).AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new ProductApiService(productRepoStub, productCategoryRepo, vendorRepo, storeMappingService);
            var result = cut.GetProductById(negativeOrZeroProductId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        public void WhenProductIsReturnedByTheRepository_ShouldReturnTheSameProduct()
        {
            int productId = 3;
            var product = new Product() { Id = 3, Name = "some name" };

            // Arange
            var productRepo = Substitute.For<IRepository<Product>>();
            productRepo.GetById(productId).Returns(product);

            var list = new List<Product>();
            list.Add(product);

            productRepo.Table.Returns(list.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            var vendorRepo = Substitute.For<IRepository<Vendor>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new ProductApiService(productRepo, productCategoryRepo, vendorRepo, storeMappingService);
            var result = cut.GetProductById(productId);

            // Assert
            ClassicAssert.AreSame(product, result);
        }
    }
}