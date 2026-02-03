using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTOs.GiftCards
{
    [JsonObject(Title = "gift_card_usage_history")]
    public class GiftCardUsageHistoryDto : BaseDto
    {
        /// <summary>
        ///     Gets or sets the gift card identifier
        /// </summary>
        [JsonProperty("gift_card_id")]
        public int? GiftCardId { get; set; }

        /// <summary>
        ///     Gets or sets the order identifier where this gift card was used
        /// </summary>
        [JsonProperty("used_with_order_id")]
        public int? UsedWithOrderId { get; set; }

        /// <summary>
        ///     Gets or sets the used value (amount)
        /// </summary>
        [JsonProperty("used_value")]
        public decimal? UsedValue { get; set; }

        /// <summary>
        ///     Gets or sets the date and time when the gift card was used
        /// </summary>
        [JsonProperty("created_on_utc")]
        public DateTime? CreatedOnUtc { get; set; }

        /// <summary>
        ///     Gets or sets the custom order number (for display purposes)
        /// </summary>
        [JsonProperty("custom_order_number")]
        public string CustomOrderNumber { get; set; }
    }
}
