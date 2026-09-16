using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTOs.TaxRates
{
    [JsonObject(Title = "tax_rate")]
    public class TaxRateDto : BaseDto
    {
        /// <summary>
        /// Gets or sets the store identifier (0 = all stores)
        /// </summary>
        [JsonProperty("store_id")]
        public int? StoreId { get; set; }

        /// <summary>
        /// Gets or sets the tax category identifier
        /// </summary>
        [JsonProperty("tax_category_id")]
        public int? TaxCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        [JsonProperty("country_id")]
        public int? CountryId { get; set; }

        /// <summary>
        /// Gets or sets the state/province identifier (0 = all states/provinces)
        /// </summary>
        [JsonProperty("state_province_id")]
        public int? StateProvinceId { get; set; }

        /// <summary>
        /// Gets or sets the zip / postal code (empty = all zips)
        /// </summary>
        [JsonProperty("zip")]
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the tax percentage
        /// </summary>
        [JsonProperty("percentage")]
        public decimal? Percentage { get; set; }
    }
}
