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
    public class CategoryApiServiceTests_GetCategoriesCount_DefaultParameters
    {
        [Test]
        public void WhenCalledWithDefaultParameters_GivenNoCategoriesExist_ShouldReturnZero()
        {
            // Arange
            var categoryRepo = Substitute.For<IRepository<Category>>();
            categoryRepo.TableNoTracking.Returns(new List<Category>().AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Category>.Is.Anything).Returns(true);

            // Act
            var cut = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
            var categoriesCount = cut.GetCategoriesCount();

            // Assert
            ClassicAssert.AreEqual(0, categoriesCount);
        }
        
        [Test]
        public void WhenCalledWithDefaultParameters_GivenOnlyDeletedCategoriesExist_ShouldReturnZero()
        {
            var existingCategories = new List<Category>();
            existingCategories.Add(new Category() { Id = 1, Deleted = true });
            existingCategories.Add(new Category() { Id = 2, Deleted = true });

            // Arange
            var categoryRepo = Substitute.For<IRepository<Category>>();
            categoryRepo.TableNoTracking.Returns(existingCategories.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();

            // Act
            var cut = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
            var countResult = cut.GetCategoriesCount();

            // Assert
            ClassicAssert.AreEqual(0, countResult);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeNotDeletedCategoriesExist_ShouldReturnTheirCount()
        {
            var existingCategories = new List<Category>();
            existingCategories.Add(new Category() { Id = 1 });
            existingCategories.Add(new Category() { Id = 2, Deleted = true });
            existingCategories.Add(new Category() { Id = 3 });

            // Arange
            var categoryRepo = Substitute.For<IRepository<Category>>();
            categoryRepo.TableNoTracking.Returns(existingCategories.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Category>.Is.Anything).Returns(true);

            // Act
            var cut = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
            var countResult = cut.GetCategoriesCount();

            // Assert
            ClassicAssert.AreEqual(2, countResult);
        }

        [Test]
        public void WhenCalledWithDefaultParameters_GivenSomeCategoriesExist_ShouldReturnTheirCount()
        {
            var existingCategories = new List<Category>();
            existingCategories.Add(new Category() { Id = 2, Published = false });
            existingCategories.Add(new Category() { Id = 3 });
            existingCategories.Add(new Category() { Id = 1 });
            
            // Arange
            var categoryRepo = Substitute.For<IRepository<Category>>();
            categoryRepo.TableNoTracking.Returns(existingCategories.AsQueryable());

            var productCategoryRepo = Substitute.For<IRepository<ProductCategory>>();

            var storeMappingService = Substitute.For<IStoreMappingService>();
            storeMappingService.Authorize(Arg<Category>.Is.Anything).Returns(true);

            // Act
            var cut = new CategoryApiService(categoryRepo, productCategoryRepo, storeMappingService);
            var countResult = cut.GetCategoriesCount();

            // Assert
            ClassicAssert.AreEqual(existingCategories.Count, countResult);
        }
    }
}
