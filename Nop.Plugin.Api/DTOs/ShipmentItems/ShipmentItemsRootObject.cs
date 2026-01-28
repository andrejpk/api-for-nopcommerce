using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.DTOs.ShipmentItems
{
    public class ShipmentItemsRootObject : ISerializableObject
    {
        [JsonProperty("shipment_items")]
        public IList<ShipmentItemDto> ShipmentItems { get; set; } = new List<ShipmentItemDto>();
        public string GetPrimaryPropertyName() => "shipment_items";
        public Type GetPrimaryPropertyType() => typeof(ShipmentItemDto);
    }
}
