using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.TaxRates
{
    public class TaxRatesCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
