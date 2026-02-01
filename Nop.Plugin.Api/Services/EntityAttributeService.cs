using Nop.Core;
using Nop.Plugin.Api.Helpers;
using Nop.Services.Common;

namespace Nop.Plugin.Api.Services
{
    /// <summary>
    /// Service for managing entity attributes in the API
    /// </summary>
    public class EntityAttributeService : IEntityAttributeService
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IJsonHelper _jsonHelper;

        public EntityAttributeService(
            IGenericAttributeService genericAttributeService,
            IJsonHelper jsonHelper)
        {
            _genericAttributeService = genericAttributeService;
            _jsonHelper = jsonHelper;
        }

        /// <summary>
        /// Loads attributes from the database for an entity
        /// </summary>
        public async Task<Dictionary<string, string>> GetAttributesAsync<TEntity>(int entityId) where TEntity : BaseEntity
        {
            var entityTypeName = typeof(TEntity).Name;
            var attributes = await _genericAttributeService.GetAttributesForEntityAsync(entityId, entityTypeName);
            
            if (attributes == null || !attributes.Any())
                return null;

            return attributes.ToDictionary(
                attr => attr.Key,
                attr => attr.Value
            );
        }

        /// <summary>
        /// Saves attributes to the database for an entity
        /// </summary>
        public async Task SaveAttributesAsync<TEntity>(TEntity entity, Dictionary<string, string> attributes) where TEntity : BaseEntity
        {
            if (attributes == null || !attributes.Any())
                return;

            foreach (var attribute in attributes)
            {
                await _genericAttributeService.SaveAttributeAsync(entity, attribute.Key, attribute.Value);
            }
        }

        /// <summary>
        /// Extracts attributes from the raw JSON request body for a specific root key
        /// </summary>
        public Dictionary<string, string> ExtractAttributesFromJson(Stream requestBody, string rootKey)
        {
            try
            {
                // Reset stream position to beginning
                if (requestBody.CanSeek)
                    requestBody.Position = 0;

                // Read the raw JSON from the request body
                var requestJson = _jsonHelper.GetRequestJsonDictionaryFromStream(requestBody, true);
                
                // Get the root object from the JSON (e.g., "shipment", "order", etc.)
                if (requestJson != null && requestJson.ContainsKey(rootKey))
                {
                    var rootJson = requestJson[rootKey] as Dictionary<string, object>;
                    
                    // Get the attributes object
                    if (rootJson != null && rootJson.ContainsKey("attributes"))
                    {
                        var attributesObj = rootJson["attributes"] as Dictionary<string, object>;
                        if (attributesObj != null)
                        {
                            // Convert Dictionary<string, object> to Dictionary<string, string>
                            return attributesObj.ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value?.ToString() ?? string.Empty
                            );
                        }
                    }
                }
            }
            catch
            {
                // If extraction fails, return null (attributes are optional)
            }

            return null;
        }
    }
}
