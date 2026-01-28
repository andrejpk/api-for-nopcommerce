using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Api.Delta;
using System.Net;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.DTO.OrderItems;
using Nop.Plugin.Api.DTOs.Shipments;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.ShipmentsParameters;

namespace Nop.Plugin.Api.Controllers
{
    [AuthorizePermission(nameof(StandardPermissionProvider.ManageOrders))]
    public class ShipmentsController(
        IJsonFieldsSerializer jsonFieldsSerializer,
        IAclService aclService,
        ICustomerService customerService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IDiscountService discountService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IOrderApiService orderApiService,
        IShipmentApiService shipmentApiService,
        IPictureService pictureService,
        IDTOHelper dtoHelper)
        : BaseApiController(jsonFieldsSerializer,
            aclService,
            customerService,
            storeMappingService,
            storeService,
            discountService,
            customerActivityService,
            localizationService,
            pictureService)
    {
        [HttpGet]
        [Route("/api/orders/{orderId}/shipments", Name = "GetOrderShipments")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetOrderShipments([FromRoute] int orderId,
            [FromQuery] ShipmentsParametersModel parameters)
        {
            if (parameters.Limit is < Constants.Configurations.MinLimit or > Constants.Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "Invalid limit parameter");
            }

            if (parameters.Page < Constants.Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid request parameters");
            }

            var order = orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var allShipmentsForOrder = await shipmentApiService.GetShipmentsForOrderAsync(order, parameters.Limit,
                parameters.Page,
                parameters.SinceId);

            var shipmentsRootObject = new ShipmentsRootObject
            {
                Shipments = await allShipmentsForOrder
                    .SelectAwait(async item => await dtoHelper.PrepareShipmentDTOAsync(item)).ToListAsync()
            };

            var json = JsonFieldsSerializer.Serialize(shipmentsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/orders/{orderId}/shipments/{shipmentId}", Name = "GetShipmentByIdForOrder")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetShipmentByIdForOrder([FromRoute] int orderId, [FromRoute] int shipmentId,
            [FromQuery] string fields = "")
        {
            var order = orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var shipment = await shipmentApiService.GetShipmentByIdAsync(shipmentId);

            if (shipment == null)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            var shipmentDtos = new List<ShipmentDto>
            {
                await dtoHelper.PrepareShipmentDTOAsync(shipment)
            };

            var shipmentsRootObject = new ShipmentsRootObject
            {
                Shipments = shipmentDtos
            };

            var json = JsonFieldsSerializer.Serialize(shipmentsRootObject, fields);

            return new RawJsonActionResult(json);
        }
        
        [HttpPost]
        [Route("/api/orders/{orderId}/shipments", Name = "CreateShipment")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateShipment(
            [FromRoute]
            int orderId,
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<ShipmentDto>))]
            Delta<ShipmentDto> shipmentDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }
            
            var order = orderApiService.GetOrderById(orderId);
            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }
            
            var newShipment = new Shipment
            {
                OrderId = orderId,
                CreatedOnUtc = DateTime.UtcNow
            };
            shipmentDelta.Merge(newShipment);
            await shipmentApiService.InsertShipmentAsync(newShipment);
            
            var shipmentItems = shipmentDelta.Dto.ShipmentItems.Select(dto => new ShipmentItem
            {
                OrderItemId = dto.OrderItemId,
                Quantity = dto.Quantity,
                WarehouseId = dto.WarehouseId,
                ShipmentId = newShipment.Id
            }).ToList();
            await shipmentApiService.InsertShipmentItemsAsync(shipmentItems);
            
            await CustomerActivityService.InsertActivityAsync("AddNewShipment", await LocalizationService.GetResourceAsync("ActivityLog.AddNewShipment"), newShipment);
            var orderItemsRootObject = new ShipmentsRootObject();
            var shipmentDto = await dtoHelper.PrepareShipmentDTOAsync(newShipment);
            orderItemsRootObject.Shipments.Add(shipmentDto);
            var json = JsonFieldsSerializer.Serialize(orderItemsRootObject, string.Empty);
            return new RawJsonActionResult(json);
        }
        
        [HttpPut]
        [Route("/api/orders/{orderId}/shipments/{shipmentId}", Name = "UpdateShipment")]
        [ProducesResponseType(typeof(ShipmentsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> UpdateShipment([FromRoute] int orderId, [FromRoute] int shipmentId,
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<ShipmentDto>))]
            Delta<ShipmentDto> shipmentDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }


            var order = orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }
            
            var shipmentToUpdate = await shipmentApiService.GetShipmentByIdAsync(shipmentId);
            if (shipmentToUpdate == null || shipmentToUpdate.OrderId != orderId)
            {
                return Error(HttpStatusCode.NotFound, "shipment", "not found");
            }

            // This is needed because those fields shouldn't be updatable. That is why we save them and after the merge set them back.
            var createdOnUtc = shipmentToUpdate.CreatedOnUtc;

            shipmentDelta.Merge(shipmentToUpdate);

            shipmentToUpdate.CreatedOnUtc = createdOnUtc;

            await shipmentApiService.UpdateShipmentAsync(shipmentToUpdate);
            await CustomerActivityService.InsertActivityAsync("UpdateShipment", await LocalizationService.GetResourceAsync("ActivityLog.UpdateShipment"), shipmentToUpdate);

            var shipmentsRootObject = new ShipmentsRootObject();

            shipmentsRootObject.Shipments.Add(shipmentToUpdate.ToDto());

            var json = JsonFieldsSerializer.Serialize(shipmentsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }
        
        [HttpDelete]
        [Route("/api/orders/{orderId}/shipments/{shipmentId}", Name = "DeleteShipmentById")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteShipmentById([FromRoute] int orderId, [FromRoute] int shipmentId)
        {
            var order = orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var shipment = await shipmentApiService.GetShipmentByIdAsync(shipmentId);
            await shipmentApiService.DeleteShipmentAsync(shipment);

            return new RawJsonActionResult("{}");
        }
    }
}