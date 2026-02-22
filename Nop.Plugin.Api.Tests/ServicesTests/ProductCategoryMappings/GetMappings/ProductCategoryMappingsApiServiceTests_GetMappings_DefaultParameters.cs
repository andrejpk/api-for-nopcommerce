using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using static Nop.Plugin.Api.Infrastructure.Constants;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.ProductCategoryMappings.GetMappings
{
    [TestFixture]
    public class ProductCategoryMappingsApiServiceTests_GetMappings_DefaultParameters
    {
        [Test]
        public void GivenNonEmptyValidRepositoryWithMoreThanTheMaxItems_ShouldReturnDefaultLimitItems()
        {
            var repo = new List<ProductCategory>();

            var randomNumber = new Random();

            var currentRepoSize = Constants.Configurations.MaxLimit * 2;

            for (int i = 0; i < currentRepoSize; i++)
            {
                repo.Add(new ProductCategory()
                {
                    CategoryId = randomNumber.Next(10, 20),
                    ProductId = randomNumber.Next(1, 2),
                });
            }

            // Arange
            var mappingRepo = Substitute.For<IRepository<ProductCategory>>();
            mappingRepo.TableNoTracking.Returns(repo.AsQueryable());

            // Act
            var cut = new ProductCategoryMappingsApiService(mappingRepo);

            var result = cut.GetMappings();

            // Assert
            ClassicAssert.IsNotEmpty(result);
            ClassicAssert.AreEqual(Constants.Configurations.DefaultLimit, result.Count);
        }

        [Test]
        public void GivenEmptyRepository_ShouldReturnEmptyCollection()
        {
            var repo = new List<ProductCategory>();
           
            // Arange
            var mappingRepo = Substitute.For<IRepository<ProductCategory>>();
            mappingRepo.TableNoTracking.Returns(repo.AsQueryable());

            // Act
            var cut = new ProductCategoryMappingsApiService(mappingRepo);

            var result = cut.GetMappings();

            // Assert
            ClassicAssert.IsEmpty(result);
        }
    }
}