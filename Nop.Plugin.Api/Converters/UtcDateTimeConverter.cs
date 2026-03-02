using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Api.Converters
{
    /// <summary>
    /// Custom JSON converter that ensures DateTime values are always treated as UTC.
    /// This prevents timezone-related bugs when the host application (NopCommerce)
    /// doesn't configure DateTimeZoneHandling.Utc globally.
    /// </summary>
    public class UtcDateTimeConverter : IsoDateTimeConverter
    {
        public UtcDateTimeConverter()
        {
            DateTimeStyles = System.Globalization.DateTimeStyles.AdjustToUniversal;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dateTime = base.ReadJson(reader, objectType, existingValue, serializer);

            if (dateTime is DateTime dt && dt.Kind != DateTimeKind.Utc)
            {
                // Ensure the DateTime has Utc kind
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }

            return dateTime;
        }
    }
}
