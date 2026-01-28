using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.OrderItems;

namespace Nop.Plugin.Api.DTOs.Shipments
{
    public class ShipmentsRootObject : ISerializableObject
    {
        [JsonProperty("shipments")]
        public IList<ShipmentDto> Shipments { get; set; } = new List<ShipmentDto>();
        public string GetPrimaryPropertyName() => "shipments";
        public Type GetPrimaryPropertyType() => typeof(ShipmentDto);
    }
}
