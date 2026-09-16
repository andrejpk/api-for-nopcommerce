using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.TaxRatesParameters
{
    // JsonProperty is used only for swagger; ParametersModelBinder maps snake_case query keys to properties
    [ModelBinder(typeof(ParametersModelBinder<TaxRatesParametersModel>))]
    public class TaxRatesParametersModel
    {
        public TaxRatesParametersModel()
        {
            Ids = null;
            SinceId = Constants.Configurations.DefaultSinceId;
            Page = Constants.Configurations.DefaultPageValue;
            Limit = Constants.Configurations.DefaultLimit;
            Fields = string.Empty;
        }

        /// <summary>
        ///     A comma-separated list of tax rate ids
        /// </summary>
        [JsonProperty("ids")]
        public List<int> Ids { get; set; }

        /// <summary>
        ///     Show only tax rates for this store (0 = rates that apply to all stores)
        /// </summary>
        [JsonProperty("store_id")]
        public int? StoreId { get; set; }

        /// <summary>
        ///     Show only tax rates for this tax category
        /// </summary>
        [JsonProperty("tax_category_id")]
        public int? TaxCategoryId { get; set; }

        /// <summary>
        ///     Show only tax rates for this country
        /// </summary>
        [JsonProperty("country_id")]
        public int? CountryId { get; set; }

        /// <summary>
        ///     Show only tax rates for this state/province (0 = rates that apply to all states)
        /// </summary>
        [JsonProperty("state_province_id")]
        public int? StateProvinceId { get; set; }

        /// <summary>
        ///     Show only tax rates with this exact zip / postal code
        /// </summary>
        [JsonProperty("zip")]
        public string Zip { get; set; }

        /// <summary>
        ///     Restrict results to after the specified ID
        /// </summary>
        [JsonProperty("since_id")]
        public int SinceId { get; set; }

        /// <summary>
        ///     Page to show (default: 1)
        /// </summary>
        [JsonProperty("page")]
        public int Page { get; set; }

        /// <summary>
        ///     Amount of results (default: 50) (maximum: 250)
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; set; }

        /// <summary>
        ///     comma-separated list of fields to include in the response
        /// </summary>
        [JsonProperty("fields")]
        public string Fields { get; set; }
    }
}
