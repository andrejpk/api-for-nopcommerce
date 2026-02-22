using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Categories.GetCategoryById
{
    using Nop.Services.Stores;

    [TestFixture]
    public class CategoryApiServiceTests_GetCategoryById
    {
        [Test]
        public void WhenNullIsReturnedByTheRepository_ShouldReturnNull()
        {
            int categoryId = 3;
            
            // Arange
            var categoryRepo = Substitute.For<IRepository<Category>>();
            categoryRepo.Table.Returns((new List<Category>()).AsQueryable());
            categoryRepo.GetById(categoryId).Returns(null);

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
            var result = cut.GetCategoryById(categoryId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        [TestCase(-2)]
        [TestCase(0)]
        public void WhenNegativeOrZeroCategoryIdPassed_ShouldReturnNull(int negativeOrZeroCategoryId)
        {
            // Aranges
            var categoryRepoStub = Substitute.For<IRepository<Category>>();
            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new CategoryApiService(categoryRepoStub, productCategoryRepo, storeMappingService);
            var result = cut.GetCategoryById(negativeOrZeroCategoryId);

            // Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        public void WhenCategoryIsReturnedByTheRepository_ShouldReturnTheSameCategory()
        {
            int categoryId = 3;
            Category category = new Category() { Id = 3, Name = "some name" };

            // Arange
            var categoryRepo = Substitute.For<IRepository<Category>>();

            var list = new List<Category>();
            list.Add(category);

            categoryRepo.Table.Returns(list.AsQueryable());

            categoryRepo.GetById(categoryId).Returns(category);

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
            var result = cut.GetCategoryById(categoryId);

            // Assert
            ClassicAssert.AreSame(category, result);
        }
    }
}
