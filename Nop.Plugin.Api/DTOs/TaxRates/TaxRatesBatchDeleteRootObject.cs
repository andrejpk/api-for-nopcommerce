using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.TaxRates
{
    public class TaxRatesBatchDeleteRootObject
    {
        public TaxRatesBatchDeleteRootObject()
        {
            DeletedIds = new List<int>();
            NotFoundIds = new List<int>();
        }

        [JsonProperty("deleted_ids")]
        public IList<int> DeletedIds { get; set; }

        [JsonProperty("not_found_ids")]
        public IList<int> NotFoundIds { get; set; }
    }
}
