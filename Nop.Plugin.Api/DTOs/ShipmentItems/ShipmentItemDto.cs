using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.OrderItems;

namespace Nop.Plugin.Api.DTOs.ShipmentItems
{
    [JsonObject(Title = "shipment_item")]
    public class ShipmentItemDto : BaseDto
    {
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        
        [JsonProperty("order_item")]
        [DoNotMap]
        public OrderItemDto OrderItem { get; set; }
        
        [JsonProperty("order_item_id")]
        public int OrderItemId { get; set; }
        
        [JsonProperty("warehouse_id")]
        public int WarehouseId { get; set; }
    }
}
