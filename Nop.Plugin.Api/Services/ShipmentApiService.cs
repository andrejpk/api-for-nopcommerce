#nullable enable
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.DataStructures;
using Nop.Services.Shipping;

namespace Nop.Plugin.Api.Services
{
    public class ShipmentApiService(IShipmentService shipmentService) : IShipmentApiService
    {
        public async Task<IList<Shipment>> GetShipmentsForOrderAsync(Order order, int limit, int page, int sinceId)
        {
            var orderShipments = (await shipmentService.GetShipmentsByOrderIdAsync(order.Id)).AsQueryable();
            return new ApiList<Shipment>(orderShipments, page - 1, limit);
        }

        public async Task<Shipment> GetShipmentByIdAsync(int shipmentId) => await shipmentService.GetShipmentByIdAsync(shipmentId);
        public async Task DeleteShipmentAsync(Shipment shipment) => await shipmentService.DeleteShipmentAsync(shipment);
        public async Task UpdateShipmentAsync(Shipment shipment) => await shipmentService.UpdateShipmentAsync(shipment);
        public async Task InsertShipmentAsync(Shipment newShipment) => 
            await shipmentService.InsertShipmentAsync(newShipment);

        public async Task InsertShipmentItemsAsync(IList<ShipmentItem> newShipmentItems)
        {
            foreach (var shipmentItem in newShipmentItems)
            {
                await shipmentService.InsertShipmentItemAsync(shipmentItem);
            }        
        }
    }
}
