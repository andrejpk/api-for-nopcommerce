using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface ITaxRateApiService
    {
        IList<TaxRate> GetTaxRates(
            IList<int> ids = null,
            int? storeId = null,
            int? taxCategoryId = null,
            int? countryId = null,
            int? stateProvinceId = null,
            string zip = null,
            int limit = Constants.Configurations.DefaultLimit,
            int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId);

        int GetTaxRatesCount(
            int? storeId = null,
            int? taxCategoryId = null,
            int? countryId = null,
            int? stateProvinceId = null,
            string zip = null);

        Task<TaxRate> GetTaxRateByIdAsync(int id);

        Task<IList<TaxRate>> GetTaxRatesByIdsAsync(IList<int> ids);

        /// <summary>
        /// Returns all tax rates, keyed by their natural key
        /// (store, tax category, country, state/province, zip). Used for upsert matching.
        /// </summary>
        Task<IDictionary<string, TaxRate>> GetTaxRatesByNaturalKeyAsync();

        string GetNaturalKey(int storeId, int taxCategoryId, int countryId, int stateProvinceId, string zip);

        string NormalizeZip(string zip);

        Task InsertTaxRateAsync(TaxRate taxRate);

        Task InsertTaxRatesAsync(IList<TaxRate> taxRates);

        Task UpdateTaxRateAsync(TaxRate taxRate);

        Task UpdateTaxRatesAsync(IList<TaxRate> taxRates);

        Task DeleteTaxRateAsync(TaxRate taxRate);

        Task DeleteTaxRatesAsync(IList<TaxRate> taxRates);
    }
}
