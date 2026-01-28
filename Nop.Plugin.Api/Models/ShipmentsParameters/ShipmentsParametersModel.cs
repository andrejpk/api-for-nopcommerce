using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.ShipmentsParameters
{
    [ModelBinder(typeof(ParametersModelBinder<ShipmentsParametersModel>))]
    public class ShipmentsParametersModel
    {
        public ShipmentsParametersModel()
        {
            Limit = Constants.Configurations.DefaultLimit;
            Page = Constants.Configurations.DefaultPageValue;
            SinceId = 0;
            Fields = string.Empty;
        }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("since_id")]
        public int SinceId { get; set; }

        [JsonProperty("fields")]
        public string Fields { get; set; }
    }
}
