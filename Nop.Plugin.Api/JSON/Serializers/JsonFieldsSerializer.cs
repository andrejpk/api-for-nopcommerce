using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.Helpers;

namespace Nop.Plugin.Api.JSON.Serializers
{
    public class JsonFieldsSerializer : IJsonFieldsSerializer
    {
        private static readonly JsonSerializer _serializer = CreateSerializer();

        private static JsonSerializer CreateSerializer()
        {
            var serializer = new JsonSerializer
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            serializer.Converters.Add(new StringEnumConverter());
            return serializer;
        }

        public string Serialize(ISerializableObject objectToSerialize, string jsonFields)
        {
            if (objectToSerialize == null)
            {
                throw new ArgumentNullException(nameof(objectToSerialize));
            }

            IList<string> fieldsList = null;

            if (!string.IsNullOrEmpty(jsonFields))
            {
                var primaryPropertyName = objectToSerialize.GetPrimaryPropertyName();

                fieldsList = GetPropertiesIntoList(jsonFields);

                // Always add the root manually
                fieldsList.Add(primaryPropertyName);
            }

            var json = Serialize(objectToSerialize, fieldsList);

            return json;
        }

        private string Serialize(object objectToSerialize, IList<string> jsonFields = null)
        {
            var jToken = JToken.FromObject(objectToSerialize, _serializer);

            if (jsonFields != null)
            {
                jToken = jToken.RemoveEmptyChildrenAndFilterByFields(jsonFields);
            }

            var jTokenResult = jToken.ToString();

            return jTokenResult;
        }

        private IList<string> GetPropertiesIntoList(string fields)
        {
            IList<string> properties = fields.ToLowerInvariant()
                                             .Split(new[]
                                                    {
                                                        ','
                                                    }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(x => x.Trim())
                                             .Distinct()
                                             .ToList();

            return properties;
        }
    }
}
