using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.TaxRatesParameters
{
    public class TaxRatesBatchDeleteParametersModel
    {
        /// <summary>
        ///     Ids of the tax rates to delete
        /// </summary>
        [JsonProperty("ids")]
        public List<int> Ids { get; set; }
    }
}
