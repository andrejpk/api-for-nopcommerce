using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO.OrderItems;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class ShipmentDtoMappings
    {
        public static ShipmentDto ToDto(this Shipment shipment) => shipment.MapTo<Shipment, ShipmentDto>();
    }
}
