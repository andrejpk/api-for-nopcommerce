using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.OrderNotes
{
    public class OrderNotesRootObject : ISerializableObject
    {
        public OrderNotesRootObject()
        {
            OrderNotes = new List<OrderNoteDto>();
        }

        [JsonProperty("order_notes")]
        public IList<OrderNoteDto> OrderNotes { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "order_notes";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(OrderNoteDto);
        }
    }
}
