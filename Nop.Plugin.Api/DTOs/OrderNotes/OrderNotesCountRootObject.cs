using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.OrderNotes
{
    public class OrderNotesCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
