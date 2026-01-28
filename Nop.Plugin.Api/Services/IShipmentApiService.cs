#nullable enable
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Api.Services
{
    public interface IShipmentApiService
    {
        Task<IList<Shipment>> GetShipmentsForOrderAsync(Order order, int limit, int page, int sinceId);
        Task<Shipment> GetShipmentByIdAsync(int shipmentId);
        Task DeleteShipmentAsync(Shipment shipment);
        Task UpdateShipmentAsync(Shipment shipment);
        public Task InsertShipmentAsync(Shipment newShipment);
        public Task InsertShipmentItemsAsync(IList<ShipmentItem> newShipmentItems);
    }
}
