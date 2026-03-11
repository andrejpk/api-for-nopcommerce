using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Converters;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTOs.ShipmentItems;

namespace Nop.Plugin.Api.DTO.OrderItems
{
    [JsonObject(Title = "shipment")]
    public class ShipmentDto : BaseDto, IAttributeDto
    {
        private ICollection<ShipmentItemDto> _shipmentItems;
        private Dictionary<string, string> _attributes;
        
        /// <summary>
        ///     Gets or sets the tracking number
        /// </summary>
        [JsonProperty("tracking_number")]
        public string TrackingNumber { get; set; }
        
        /// <summary>
        ///     Gets or sets the admin comment
        /// </summary>
        [JsonProperty("admin_comment")]
        public string AdminComment { get; set; }
        
        /// <summary>
        ///     Gets or sets the weight
        /// </summary>
        [JsonProperty("weight")]
        public decimal TotalWeight { get; set; }
        
        /// <summary>
        ///     Gets or sets the created on time
        /// </summary>
        [JsonProperty("created_on_utc")]
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        ///     Gets or sets the shipped date
        /// </summary>
        [JsonProperty("shipped_date_utc")]
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime? ShippedDateUtc { get; set; }

        /// <summary>
        ///     Gets or sets the delivery date
        /// </summary>
        [JsonProperty("delivery_date_utc")]
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime? DeliveryDateUtc { get; set; }

        /// <summary>
        ///     Gets or sets the ready for pickup date
        /// </summary>
        [JsonProperty("ready_for_pickup_date_utc")]
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime? ReadyForPickupDateUtc { get; set; }
        
        /// <summary>
        ///     Gets or sets the shipment generic attributes
        ///     Note: This property is excluded from Delta merge and handled separately in the controller
        /// </summary>
        [JsonProperty("attributes")]
        [DoNotMap]
        public Dictionary<string, string> Attributes
        {
            get => _attributes ??= new Dictionary<string, string>();
            set => _attributes = value;
        }
        
        [JsonProperty("shipment_items")]
        public ICollection<ShipmentItemDto> ShipmentItems
        {
            get => _shipmentItems ??= new List<ShipmentItemDto>();
            set => _shipmentItems = value;
        }
    }
}
