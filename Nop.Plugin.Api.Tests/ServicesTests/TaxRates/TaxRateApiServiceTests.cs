using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.Services;
using NSubstitute;
using NUnit.Framework;

namespace Nop.Plugin.Api.Tests.ServicesTests.TaxRates
{
    [TestFixture]
    public class TaxRateApiServiceTests
    {
        private IRepository<TaxRate> _repository;
        private IStaticCacheManager _staticCacheManager;
        private TaxRateApiService _service;
        private List<TaxRate> _rows;

        [SetUp]
        public void SetUp()
        {
            _rows = new List<TaxRate>
            {
                new() { Id = 1, StoreId = 0, TaxCategoryId = 1, CountryId = 1, StateProvinceId = 0, Zip = "", Percentage = 5m },
                new() { Id = 2, StoreId = 0, TaxCategoryId = 1, CountryId = 1, StateProvinceId = 10, Zip = "", Percentage = 6m },
                new() { Id = 3, StoreId = 0, TaxCategoryId = 1, CountryId = 1, StateProvinceId = 10, Zip = "90210", Percentage = 7m },
                new() { Id = 4, StoreId = 2, TaxCategoryId = 2, CountryId = 3, StateProvinceId = 0, Zip = null, Percentage = 8m },
            };

            _repository = Substitute.For<IRepository<TaxRate>>();
            _repository.Table.Returns(_rows.AsQueryable());
            _repository
                .GetAllAsync(Arg.Any<Func<IQueryable<TaxRate>, IQueryable<TaxRate>>>(), Arg.Any<Func<ICacheKeyService, CacheKey>>(), Arg.Any<bool>())
                .Returns(callInfo =>
                {
                    var func = callInfo.ArgAt<Func<IQueryable<TaxRate>, IQueryable<TaxRate>>>(0);
                    IQueryable<TaxRate> query = _rows.AsQueryable();
                    return Task.FromResult<IList<TaxRate>>((func != null ? func(query) : query).ToList());
                });

            _staticCacheManager = Substitute.For<IStaticCacheManager>();

            _service = new TaxRateApiService(_repository, _staticCacheManager);
        }

        [Test]
        public void GetTaxRates_WithoutFilters_ReturnsAllOrderedById()
        {
            var result = _service.GetTaxRates();

            Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void GetTaxRates_FiltersByCountryAndState()
        {
            var result = _service.GetTaxRates(countryId: 1, stateProvinceId: 10);

            Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 2, 3 }));
        }

        [Test]
        public void GetTaxRates_FiltersByStoreZeroMeaningAllStores()
        {
            var result = _service.GetTaxRates(storeId: 0);

            Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void GetTaxRates_FiltersByIds()
        {
            var result = _service.GetTaxRates(ids: new List<int> { 4, 2 });

            Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 2, 4 }));
        }

        [Test]
        public void GetTaxRates_ZipFilterIsTrimmed()
        {
            var result = _service.GetTaxRates(zip: " 90210 ");

            Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 3 }));
        }

        [Test]
        public void GetTaxRates_Paginates()
        {
            var page1 = _service.GetTaxRates(limit: 2, page: 1);
            var page2 = _service.GetTaxRates(limit: 2, page: 2);

            Assert.That(page1.Select(x => x.Id), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(page2.Select(x => x.Id), Is.EqualTo(new[] { 3, 4 }));
        }

        [Test]
        public void GetTaxRates_SinceId()
        {
            var result = _service.GetTaxRates(sinceId: 2);

            Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 3, 4 }));
        }

        [Test]
        public void GetTaxRatesCount_AppliesFilters()
        {
            Assert.That(_service.GetTaxRatesCount(), Is.EqualTo(4));
            Assert.That(_service.GetTaxRatesCount(taxCategoryId: 1), Is.EqualTo(3));
        }

        [Test]
        public void GetNaturalKey_IgnoresZipCaseWhitespaceAndNull()
        {
            var a = _service.GetNaturalKey(0, 1, 1, 0, null);
            var b = _service.GetNaturalKey(0, 1, 1, 0, "  ");
            var c = _service.GetNaturalKey(0, 1, 1, 0, "ab 1");
            var d = _service.GetNaturalKey(0, 1, 1, 0, " AB 1 ");

            Assert.That(a, Is.EqualTo(b));
            Assert.That(c, Is.EqualTo(d));
            Assert.That(a, Is.Not.EqualTo(c));
        }

        [Test]
        public async Task GetTaxRatesByNaturalKeyAsync_IndexesEveryRow()
        {
            var index = await _service.GetTaxRatesByNaturalKeyAsync();

            Assert.That(index.Count, Is.EqualTo(4));
            Assert.That(index[_service.GetNaturalKey(0, 1, 1, 10, "90210")].Id, Is.EqualTo(3));
            Assert.That(index[_service.GetNaturalKey(2, 2, 3, 0, "")].Id, Is.EqualTo(4));
        }

        [Test]
        public async Task GetTaxRatesByNaturalKeyAsync_KeepsLowestIdOnDuplicates()
        {
            _rows.Add(new TaxRate { Id = 5, StoreId = 0, TaxCategoryId = 1, CountryId = 1, StateProvinceId = 0, Zip = "", Percentage = 9m });

            var index = await _service.GetTaxRatesByNaturalKeyAsync();

            Assert.That(index[_service.GetNaturalKey(0, 1, 1, 0, "")].Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetTaxRateByIdAsync_InvalidId_ReturnsNull()
        {
            Assert.That(await _service.GetTaxRateByIdAsync(0), Is.Null);
            Assert.That(await _service.GetTaxRateByIdAsync(-1), Is.Null);
        }

        [Test]
        public async Task GetTaxRatesByIdsAsync_EmptyIds_DoesNotHitRepository()
        {
            var result = await _service.GetTaxRatesByIdsAsync(new List<int>());

            Assert.That(result, Is.Empty);
            await _repository.DidNotReceive().GetByIdsAsync(Arg.Any<IList<int>>(), Arg.Any<Func<ICacheKeyService, CacheKey>>(), Arg.Any<bool>());
        }

        [Test]
        public async Task InsertTaxRatesAsync_BulkInsertsAndClearsTaxPluginCache()
        {
            var rates = new List<TaxRate> { new() { CountryId = 1, TaxCategoryId = 1 } };

            await _service.InsertTaxRatesAsync(rates);

            await _repository.Received(1).InsertAsync(rates, Arg.Any<bool>());
            await _staticCacheManager.Received().RemoveByPrefixAsync("Nop.plugins.tax.fixedorbycountrystateziptaxrate.");
        }

        [Test]
        public async Task InsertTaxRatesAsync_EmptyList_DoesNothing()
        {
            await _service.InsertTaxRatesAsync(new List<TaxRate>());

            await _repository.DidNotReceive().InsertAsync(Arg.Any<IList<TaxRate>>(), Arg.Any<bool>());
        }

        [Test]
        public async Task UpdateAndDelete_ClearTaxPluginCache()
        {
            var rate = _rows[0];

            await _service.UpdateTaxRateAsync(rate);
            await _service.DeleteTaxRateAsync(rate);

            await _repository.Received(1).UpdateAsync(rate, Arg.Any<bool>());
            await _repository.Received(1).DeleteAsync(rate, Arg.Any<bool>());
            await _staticCacheManager.Received(2).RemoveByPrefixAsync("Nop.plugins.tax.fixedorbycountrystateziptaxrate.");
        }
    }
}
