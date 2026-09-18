using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Api.Controllers;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTOs.TaxRates;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.TaxRatesParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using NSubstitute;
using NUnit.Framework;

namespace Nop.Plugin.Api.Tests.ControllersTests.TaxRates
{
    [TestFixture]
    public class TaxRatesControllerBatchTests
    {
        private ITaxRateApiService _taxRateApiService;
        private ITaxCategoryService _taxCategoryService;
        private ICountryService _countryService;
        private IStateProvinceService _stateProvinceService;
        private IStoreService _storeService;
        private IJsonFieldsSerializer _serializer;
        private TaxRatesController _controller;

        private Dictionary<string, TaxRate> _index;
        private List<TaxRate> _byIds;

        [SetUp]
        public void SetUp()
        {
            _taxRateApiService = Substitute.For<ITaxRateApiService>();
            _taxCategoryService = Substitute.For<ITaxCategoryService>();
            _countryService = Substitute.For<ICountryService>();
            _stateProvinceService = Substitute.For<IStateProvinceService>();
            _storeService = Substitute.For<IStoreService>();
            _serializer = Substitute.For<IJsonFieldsSerializer>();
            _serializer.Serialize(Arg.Any<ISerializableObject>(), Arg.Any<string>()).Returns("{}");

            // reference data: tax category 1, country 1 with state 10, country 2; store 5
            _taxCategoryService.GetTaxCategoryByIdAsync(1).Returns(new TaxCategory { Id = 1 });
            _countryService.GetCountryByIdAsync(1).Returns(new Country { Id = 1 });
            _countryService.GetCountryByIdAsync(2).Returns(new Country { Id = 2 });
            _stateProvinceService.GetStateProvinceByIdAsync(10).Returns(new StateProvince { Id = 10, CountryId = 1 });
            _storeService.GetStoreByIdAsync(5).Returns(new Store { Id = 5 });

            // real key semantics from the service
            var realService = new TaxRateApiService(Substitute.For<Nop.Data.IRepository<TaxRate>>(), Substitute.For<Nop.Core.Caching.IStaticCacheManager>());
            _taxRateApiService.NormalizeZip(Arg.Any<string>()).Returns(ci => realService.NormalizeZip(ci.ArgAt<string>(0)));
            _taxRateApiService.GetNaturalKey(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(ci => realService.GetNaturalKey(ci.ArgAt<int>(0), ci.ArgAt<int>(1), ci.ArgAt<int>(2), ci.ArgAt<int>(3), ci.ArgAt<string>(4)));

            _index = new Dictionary<string, TaxRate>();
            _byIds = new List<TaxRate>();
            _taxRateApiService.GetTaxRatesByNaturalKeyAsync(Arg.Any<IEnumerable<int>>()).Returns(_ => Task.FromResult<IDictionary<string, TaxRate>>(_index));
            _taxRateApiService.GetTaxRatesByIdsAsync(Arg.Any<IList<int>>())
                .Returns(ci => Task.FromResult<IList<TaxRate>>(_byIds.Where(x => ci.ArgAt<IList<int>>(0).Contains(x.Id)).ToList()));

            _controller = new TaxRatesController(
                _taxRateApiService, _taxCategoryService, _countryService, _stateProvinceService,
                _serializer,
                Substitute.For<IAclService>(),
                Substitute.For<ICustomerService>(),
                Substitute.For<IStoreMappingService>(),
                _storeService,
                Substitute.For<IDiscountService>(),
                Substitute.For<ICustomerActivityService>(),
                Substitute.For<ILocalizationService>(),
                Substitute.For<IPictureService>());
        }

        private static HttpStatusCode StatusOf(IActionResult result)
        {
            var field = typeof(ErrorActionResult).GetField("_statusCode", BindingFlags.NonPublic | BindingFlags.Instance);
            return (HttpStatusCode)field.GetValue(result);
        }

        private TaxRate Existing(int id, int country, int state, string zip, decimal pct)
        {
            var rate = new TaxRate { Id = id, StoreId = 0, TaxCategoryId = 1, CountryId = country, StateProvinceId = state, Zip = zip, Percentage = pct };
            _index[_taxRateApiService.GetNaturalKey(0, 1, country, state, zip)] = rate;
            _byIds.Add(rate);
            return rate;
        }

        [Test]
        public async Task EmptyBatch_IsBadRequest()
        {
            var result = await _controller.BatchUpsertTaxRates(new TaxRatesRootObject());

            Assert.That(result, Is.InstanceOf<ErrorActionResult>());
            Assert.That(StatusOf(result), Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task NullItemAndNegativeId_AreRejectedBeforeAnyLookup()
        {
            var batch = new TaxRatesRootObject { TaxRates = new List<TaxRateDto> { null, new TaxRateDto { Id = -1 } } };

            var result = await _controller.BatchUpsertTaxRates(batch);

            Assert.That(result, Is.InstanceOf<ErrorActionResult>());
            Assert.That(StatusOf(result), Is.EqualTo((HttpStatusCode)422));
            Assert.That(_controller.ModelState.Keys, Is.EquivalentTo(new[] { "tax_rates[0]", "tax_rates[1].id" }));
            await _taxRateApiService.DidNotReceiveWithAnyArgs().GetTaxRatesByNaturalKeyAsync(default);
        }

        [Test]
        public async Task InvalidItem_FailsWholeBatch_AndWritesNothing()
        {
            var batch = new TaxRatesRootObject
            {
                TaxRates = new List<TaxRateDto>
                {
                    new() { TaxCategoryId = 1, CountryId = 1, Percentage = 5m },
                    new() { TaxCategoryId = 1, CountryId = 999, Percentage = 5m },          // unknown country
                    new() { TaxCategoryId = 1, CountryId = 2, StateProvinceId = 10, Percentage = 5m }, // state of another country
                    new() { TaxCategoryId = 1, CountryId = 1, StoreId = 7, Percentage = 5m },  // unknown store
                    new() { TaxCategoryId = 1, CountryId = 1, Percentage = -1m },
                    new() { CountryId = 1, Percentage = 1m },                               // missing tax_category_id
                    new() { Id = 42, Percentage = 1m },                                     // unknown id
                }
            };

            var result = await _controller.BatchUpsertTaxRates(batch);

            Assert.That(result, Is.InstanceOf<ErrorActionResult>());
            Assert.That(StatusOf(result), Is.EqualTo((HttpStatusCode)422));
            Assert.That(_controller.ModelState.Keys, Is.EquivalentTo(new[]
            {
                "tax_rates[1].country_id",
                "tax_rates[2].state_province_id",
                "tax_rates[3].store_id",
                "tax_rates[4].percentage",
                "tax_rates[5].tax_category_id",
                "tax_rates[6].id",
            }));
            await _taxRateApiService.DidNotReceiveWithAnyArgs().InsertTaxRatesAsync(default);
            await _taxRateApiService.DidNotReceiveWithAnyArgs().UpdateTaxRatesAsync(default);
        }

        [Test]
        public async Task ValidBatch_UpdatesById_UpdatesByNaturalKey_InsertsNew_InRequestOrder()
        {
            var byId = Existing(1, 1, 0, "", 5m);
            var byKey = Existing(2, 1, 10, "90210", 6m);

            IList<TaxRate> inserted = null, updated = null;
            _taxRateApiService.InsertTaxRatesAsync(Arg.Do<IList<TaxRate>>(x => inserted = x)).Returns(Task.CompletedTask);
            _taxRateApiService.UpdateTaxRatesAsync(Arg.Do<IList<TaxRate>>(x => updated = x)).Returns(Task.CompletedTask);

            var batch = new TaxRatesRootObject
            {
                TaxRates = new List<TaxRateDto>
                {
                    new() { Id = 1, Percentage = 5.5m },
                    new() { TaxCategoryId = 1, CountryId = 1, StateProvinceId = 10, Zip = " 90210 ", Percentage = 6.5m },
                    new() { TaxCategoryId = 1, CountryId = 2, Percentage = 7m },
                    new() { TaxCategoryId = 1, CountryId = 2, Percentage = 7.5m }, // duplicate key within the batch
                }
            };

            var result = await _controller.BatchUpsertTaxRates(batch);

            Assert.That(result, Is.InstanceOf<RawJsonActionResult>(), string.Join(",", _controller.ModelState.Keys));
            Assert.That(updated, Is.EquivalentTo(new[] { byId, byKey }));
            Assert.That(byId.Percentage, Is.EqualTo(5.5m));
            Assert.That(byKey.Percentage, Is.EqualTo(6.5m));
            Assert.That(inserted.Count, Is.EqualTo(1));
            Assert.That(inserted[0].CountryId, Is.EqualTo(2));
            Assert.That(inserted[0].Percentage, Is.EqualTo(7.5m), "second duplicate in the batch wins");
        }

        [Test]
        public async Task IdUpdateChangingKey_IsSeenByLaterIdLessItem()
        {
            var row = Existing(1, 1, 0, "", 5m);
            // simulate the natural-key lookup returning a different instance of the same row
            _index[_taxRateApiService.GetNaturalKey(0, 1, 1, 0, "")] = new TaxRate { Id = 1, TaxCategoryId = 1, CountryId = 1, Percentage = 5m };

            IList<TaxRate> inserted = null, updated = null;
            _taxRateApiService.InsertTaxRatesAsync(Arg.Do<IList<TaxRate>>(x => inserted = x)).Returns(Task.CompletedTask);
            _taxRateApiService.UpdateTaxRatesAsync(Arg.Do<IList<TaxRate>>(x => updated = x)).Returns(Task.CompletedTask);

            var batch = new TaxRatesRootObject
            {
                TaxRates = new List<TaxRateDto>
                {
                    new() { Id = 1, Zip = "12345" },
                    new() { TaxCategoryId = 1, CountryId = 1, Zip = "12345", Percentage = 9m },
                    new() { TaxCategoryId = 1, CountryId = 1, Zip = "", Percentage = 1m },
                }
            };

            var result = await _controller.BatchUpsertTaxRates(batch);

            Assert.That(result, Is.InstanceOf<RawJsonActionResult>(), string.Join(",", _controller.ModelState.Keys));
            Assert.That(updated, Is.EqualTo(new[] { row }));
            Assert.That(row.Zip, Is.EqualTo("12345"));
            Assert.That(row.Percentage, Is.EqualTo(9m));
            Assert.That(inserted.Count, Is.EqualTo(1), "old key no longer matches, so a new row is created");
            Assert.That(inserted[0].Zip, Is.EqualTo(""));
        }

        [Test]
        public async Task BatchDelete_ReportsNotFoundIds()
        {
            _byIds.Add(new TaxRate { Id = 1 });
            _byIds.Add(new TaxRate { Id = 2 });

            var result = await _controller.BatchDeleteTaxRates(new TaxRatesBatchDeleteParametersModel { Ids = new List<int> { 1, 2, 3, 2 } });

            var ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            var body = (TaxRatesBatchDeleteRootObject)ok.Value;
            Assert.That(body.DeletedIds, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(body.NotFoundIds, Is.EqualTo(new[] { 3 }));
            await _taxRateApiService.Received(1).DeleteTaxRatesAsync(Arg.Is<IList<TaxRate>>(x => x.Count == 2));
        }

        [Test]
        public async Task BatchDelete_RejectsInvalidIds()
        {
            var result = await _controller.BatchDeleteTaxRates(new TaxRatesBatchDeleteParametersModel { Ids = new List<int> { 1, 0 } });

            Assert.That(StatusOf(result), Is.EqualTo(HttpStatusCode.BadRequest));
            await _taxRateApiService.DidNotReceiveWithAnyArgs().DeleteTaxRatesAsync(default);
        }
    }
}
