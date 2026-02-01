namespace Nop.Plugin.Api.DTO.Base
{
    /// <summary>
    /// Interface for DTOs that support generic attributes
    /// </summary>
    public interface IAttributeDto
    {
        /// <summary>
        /// Gets or sets the entity attributes as key-value pairs
        /// </summary>
        Dictionary<string, string> Attributes { get; set; }
    }
}
