using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTO.OrderNotes
{
    [JsonObject(Title = "order_note")]
    public class OrderNoteDto : BaseDto
    {
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("download_id")]
        public int? DownloadId { get; set; }

        [JsonProperty("display_to_customer")]
        public bool? DisplayToCustomer { get; set; }

        [JsonProperty("created_on_utc")]
        public DateTime? CreatedOnUtc { get; set; }
    }
}
