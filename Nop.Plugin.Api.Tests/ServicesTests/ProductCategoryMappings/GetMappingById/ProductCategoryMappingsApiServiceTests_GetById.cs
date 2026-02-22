using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.ProductCategoryMappings.GetMappingById
{
    [TestFixture]
    public class ProductCategoryMappingsApiServiceTests_GetById
    {
        [Test]
        public void WhenNullIsReturnedByTheRepository_ShouldReturnNull()
        {
            int mappingId = 3;
            
            // Arange
            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();
            productCategoryRepo.GetById(mappingId).Returns(null);
            
            // Act  
            var cut = new ProductCategoryMappingsApiService(productCategoryRepo);
            var result = cut.GetById(mappingId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        [TestCase(-2)]
        [TestCase(0)]
        public void WhenNegativeOrZeroMappingIdPassed_ShouldReturnNull(int negativeOrZeroOrderId)
        {
            // Aranges
            var mappingRepoMock = Substitute.For<IRepository<ProductCategory>>();

            // Act
            var cut = new ProductCategoryMappingsApiService(mappingRepoMock);
            var result = cut.GetById(negativeOrZeroOrderId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        public void WhenMappingIsReturnedByTheRepository_ShouldReturnTheSameMapping()
        {
            int mappingId = 3;
            var mapping = new ProductCategory() { Id = 3 };

            // Arange
            var mappingRepo = Substitute.For<IRepository<ProductCategory>>();
            mappingRepo.GetById(mappingId).Returns(mapping);
            
            // Act
            var cut = new ProductCategoryMappingsApiService(mappingRepo);
            var result = cut.GetById(mappingId);

            // Assert
            ClassicAssert.AreSame(mapping, result);
        }
    }
}
