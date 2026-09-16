using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.DTOs.TaxRates
{
    public class TaxRatesRootObject : ISerializableObject
    {
        public TaxRatesRootObject()
        {
            TaxRates = new List<TaxRateDto>();
        }

        [JsonProperty("tax_rates")]
        public IList<TaxRateDto> TaxRates { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "tax_rates";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(TaxRateDto);
        }
    }
}
