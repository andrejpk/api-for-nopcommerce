using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.TaxRatesParameters
{
    public class TaxRatesCountParametersModel
    {
        /// <summary>
        ///     Count only tax rates for this store (0 = rates that apply to all stores)
        /// </summary>
        [JsonProperty("store_id")]
        public int? StoreId { get; set; }

        /// <summary>
        ///     Count only tax rates for this tax category
        /// </summary>
        [JsonProperty("tax_category_id")]
        public int? TaxCategoryId { get; set; }

        /// <summary>
        ///     Count only tax rates for this country
        /// </summary>
        [JsonProperty("country_id")]
        public int? CountryId { get; set; }

        /// <summary>
        ///     Count only tax rates for this state/province (0 = rates that apply to all states)
        /// </summary>
        [JsonProperty("state_province_id")]
        public int? StateProvinceId { get; set; }

        /// <summary>
        ///     Count only tax rates with this exact zip / postal code
        /// </summary>
        [JsonProperty("zip")]
        public string Zip { get; set; }
    }
}
