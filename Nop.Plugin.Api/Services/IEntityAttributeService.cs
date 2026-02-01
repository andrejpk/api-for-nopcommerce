using Nop.Core;

namespace Nop.Plugin.Api.Services
{
    /// <summary>
    /// Service for managing entity attributes in the API
    /// </summary>
    public interface IEntityAttributeService
    {
        /// <summary>
        /// Loads attributes from the database for an entity
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="entityId">The entity ID</param>
        /// <returns>Dictionary of attribute key-value pairs, or null if no attributes exist</returns>
        Task<Dictionary<string, string>> GetAttributesAsync<TEntity>(int entityId) where TEntity : BaseEntity;

        /// <summary>
        /// Saves attributes to the database for an entity
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="entity">The entity instance</param>
        /// <param name="attributes">Dictionary of attribute key-value pairs to save</param>
        Task SaveAttributesAsync<TEntity>(TEntity entity, Dictionary<string, string> attributes) where TEntity : BaseEntity;

        /// <summary>
        /// Extracts attributes from the raw JSON request body for a specific root key
        /// </summary>
        /// <param name="requestBody">The request body stream</param>
        /// <param name="rootKey">The root JSON key (e.g., "shipment", "order", "product")</param>
        /// <returns>Dictionary of attribute key-value pairs, or null if not found</returns>
        Dictionary<string, string> ExtractAttributesFromJson(Stream requestBody, string rootKey);
    }
}
