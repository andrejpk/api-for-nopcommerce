using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public class TaxRateApiService : ITaxRateApiService
    {
        /// <summary>
        /// Cache prefix used by Nop.Plugin.Tax.FixedOrByCountryStateZip for its tax rate caches
        /// (see its ModelCacheEventConsumer.TAXRATE_PATTERN_KEY). Because this plugin writes to the
        /// TaxRate table through its own entity type, the tax plugin's entity event consumers do not
        /// fire, so we clear its cache explicitly after every write.
        /// </summary>
        private const string TAX_PLUGIN_CACHE_PREFIX = "Nop.plugins.tax.fixedorbycountrystateziptaxrate.";

        private readonly IRepository<TaxRate> _taxRateRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        public TaxRateApiService(IRepository<TaxRate> taxRateRepository, IStaticCacheManager staticCacheManager)
        {
            _taxRateRepository = taxRateRepository;
            _staticCacheManager = staticCacheManager;
        }

        public IList<TaxRate> GetTaxRates(
            IList<int> ids = null,
            int? storeId = null,
            int? taxCategoryId = null,
            int? countryId = null,
            int? stateProvinceId = null,
            string zip = null,
            int limit = Constants.Configurations.DefaultLimit,
            int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId)
        {
            var query = GetTaxRatesQuery(ids, storeId, taxCategoryId, countryId, stateProvinceId, zip, sinceId);

            return new ApiList<TaxRate>(query, page - 1, limit);
        }

        public int GetTaxRatesCount(
            IList<int> ids = null,
            int? storeId = null,
            int? taxCategoryId = null,
            int? countryId = null,
            int? stateProvinceId = null,
            string zip = null)
        {
            return GetTaxRatesQuery(ids, storeId, taxCategoryId, countryId, stateProvinceId, zip).Count();
        }

        public Task<TaxRate> GetTaxRateByIdAsync(int id)
        {
            if (id <= 0)
            {
                return Task.FromResult<TaxRate>(null);
            }

            return _taxRateRepository.GetByIdAsync(id);
        }

        public async Task<IList<TaxRate>> GetTaxRatesByIdsAsync(IList<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<TaxRate>();
            }

            return await _taxRateRepository.GetByIdsAsync(ids);
        }

        public async Task<IDictionary<string, TaxRate>> GetTaxRatesByNaturalKeyAsync(IEnumerable<int> countryIds = null)
        {
            var countryIdList = countryIds?.Distinct().ToList();

            // getCacheKey: null => bypass the repository cache so we always match against current rows
            var all = await _taxRateRepository.GetAllAsync(query =>
            {
                if (countryIdList != null)
                {
                    query = query.Where(tr => countryIdList.Contains(tr.CountryId));
                }

                return query.OrderBy(tr => tr.Id);
            }, getCacheKey: null);

            var result = new Dictionary<string, TaxRate>(StringComparer.Ordinal);
            foreach (var taxRate in all)
            {
                var key = GetNaturalKey(taxRate.StoreId, taxRate.TaxCategoryId, taxRate.CountryId, taxRate.StateProvinceId, taxRate.Zip);
                // if duplicates already exist in the table keep the first (lowest id) one
                result.TryAdd(key, taxRate);
            }

            return result;
        }

        public string GetNaturalKey(int storeId, int taxCategoryId, int countryId, int stateProvinceId, string zip)
        {
            return $"{storeId}|{taxCategoryId}|{countryId}|{stateProvinceId}|{NormalizeZip(zip).ToUpperInvariant()}";
        }

        public string NormalizeZip(string zip)
        {
            return zip?.Trim() ?? string.Empty;
        }

        public async Task InsertTaxRateAsync(TaxRate taxRate)
        {
            await _taxRateRepository.InsertAsync(taxRate);
            await ClearTaxPluginCacheAsync();
        }

        public async Task InsertTaxRatesAsync(IList<TaxRate> taxRates)
        {
            if (taxRates == null || taxRates.Count == 0)
            {
                return;
            }

            await _taxRateRepository.InsertAsync(taxRates);
            await ClearTaxPluginCacheAsync();
        }

        public async Task UpdateTaxRateAsync(TaxRate taxRate)
        {
            await _taxRateRepository.UpdateAsync(taxRate);
            await ClearTaxPluginCacheAsync();
        }

        public async Task UpdateTaxRatesAsync(IList<TaxRate> taxRates)
        {
            if (taxRates == null || taxRates.Count == 0)
            {
                return;
            }

            await _taxRateRepository.UpdateAsync(taxRates);
            await ClearTaxPluginCacheAsync();
        }

        public async Task DeleteTaxRateAsync(TaxRate taxRate)
        {
            await _taxRateRepository.DeleteAsync(taxRate);
            await ClearTaxPluginCacheAsync();
        }

        public async Task DeleteTaxRatesAsync(IList<TaxRate> taxRates)
        {
            if (taxRates == null || taxRates.Count == 0)
            {
                return;
            }

            await _taxRateRepository.DeleteAsync(taxRates);
            await ClearTaxPluginCacheAsync();
        }

        private async Task ClearTaxPluginCacheAsync()
        {
            await _staticCacheManager.RemoveByPrefixAsync(TAX_PLUGIN_CACHE_PREFIX);
            await _staticCacheManager.RemoveByPrefixAsync(NopEntityCacheDefaults<TaxRate>.Prefix);
        }

        private IQueryable<TaxRate> GetTaxRatesQuery(
            IList<int> ids = null,
            int? storeId = null,
            int? taxCategoryId = null,
            int? countryId = null,
            int? stateProvinceId = null,
            string zip = null,
            int sinceId = Constants.Configurations.DefaultSinceId)
        {
            var query = _taxRateRepository.Table;

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(tr => ids.Contains(tr.Id));
            }

            if (storeId != null)
            {
                query = query.Where(tr => tr.StoreId == storeId);
            }

            if (taxCategoryId != null)
            {
                query = query.Where(tr => tr.TaxCategoryId == taxCategoryId);
            }

            if (countryId != null)
            {
                query = query.Where(tr => tr.CountryId == countryId);
            }

            if (stateProvinceId != null)
            {
                query = query.Where(tr => tr.StateProvinceId == stateProvinceId);
            }

            if (zip != null)
            {
                var normalizedZip = NormalizeZip(zip);
                // an empty zip means "all zips"; rows written by the tax plugin may store that as NULL
                query = normalizedZip.Length == 0
                    ? query.Where(tr => tr.Zip == null || tr.Zip == string.Empty)
                    : query.Where(tr => tr.Zip == normalizedZip);
            }

            if (sinceId > 0)
            {
                query = query.Where(tr => tr.Id > sinceId);
            }

            query = query.OrderBy(tr => tr.Id);

            return query;
        }
    }
}
