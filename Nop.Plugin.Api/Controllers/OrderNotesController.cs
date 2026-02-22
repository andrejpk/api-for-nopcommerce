using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.OrderNotes;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.OrderNotesParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    [AuthorizePermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
    public class OrderNotesController : BaseApiController
    {
        private readonly IDTOHelper _dtoHelper;
        private readonly IOrderApiService _orderApiService;
        private readonly IOrderNoteApiService _orderNoteApiService;

        public OrderNotesController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IOrderNoteApiService orderNoteApiService,
            IOrderApiService orderApiService,
            IPictureService pictureService,
            IDTOHelper dtoHelper)
            : base(jsonFieldsSerializer,
                   aclService,
                   customerService,
                   storeMappingService,
                   storeService,
                   discountService,
                   customerActivityService,
                   localizationService,
                   pictureService)
        {
            _orderNoteApiService = orderNoteApiService;
            _orderApiService = orderApiService;
            _dtoHelper = dtoHelper;
        }

        [HttpGet]
        [Route("/api/orders/{orderId}/notes", Name = "GetOrderNotes")]
        [ProducesResponseType(typeof(OrderNotesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetOrderNotes([FromRoute] int orderId, [FromQuery] OrderNotesParametersModel parameters)
        {
            if (parameters.Limit < Constants.Configurations.MinLimit || parameters.Limit > Constants.Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "Invalid limit parameter");
            }

            if (parameters.Page < Constants.Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid request parameters");
            }

            var order = _orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var allOrderNotesForOrder = await _orderNoteApiService.GetOrderNotesForOrderAsync(order, parameters.Limit, parameters.Page, parameters.SinceId);

            var orderNotesRootObject = new OrderNotesRootObject
            {
                OrderNotes = await allOrderNotesForOrder.SelectAwait(async note => await _dtoHelper.PrepareOrderNoteDTOAsync(note)).ToListAsync()
            };

            var json = JsonFieldsSerializer.Serialize(orderNotesRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/orders/{orderId}/notes/count", Name = "GetOrderNotesCount")]
        [ProducesResponseType(typeof(OrderNotesCountRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetOrderNotesCount([FromRoute] int orderId)
        {
            var order = _orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var orderNotesCountForOrder = await _orderNoteApiService.GetOrderNotesCountAsync(order);

            var orderNotesCountRootObject = new OrderNotesCountRootObject
            {
                Count = orderNotesCountForOrder
            };

            return Ok(orderNotesCountRootObject);
        }

        [HttpGet]
        [Route("/api/orders/{orderId}/notes/{orderNoteId}", Name = "GetOrderNoteByIdForOrder")]
        [ProducesResponseType(typeof(OrderNotesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetOrderNoteByIdForOrder([FromRoute] int orderId, [FromRoute] int orderNoteId, [FromQuery] string fields = "")
        {
            var order = _orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var orderNote = await _orderNoteApiService.GetOrderNoteByIdAsync(orderNoteId);

            if (orderNote == null || orderNote.OrderId != order.Id)
            {
                return Error(HttpStatusCode.NotFound, "order_note", "not found");
            }

            var orderNoteDtos = new List<OrderNoteDto>
                                {
                                    await _dtoHelper.PrepareOrderNoteDTOAsync(orderNote)
                                };

            var orderNotesRootObject = new OrderNotesRootObject
            {
                OrderNotes = orderNoteDtos
            };

            var json = JsonFieldsSerializer.Serialize(orderNotesRootObject, fields);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/orders/{orderId}/notes", Name = "CreateOrderNote")]
        [ProducesResponseType(typeof(OrderNotesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateOrderNote(
            [FromRoute] int orderId,
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<OrderNoteDto>))]
            Delta<OrderNoteDto> orderNoteDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var order = _orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var newOrderNote = new OrderNote
            {
                OrderId = order.Id,
                Note = string.Empty,
                DownloadId = 0,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            orderNoteDelta.Merge(newOrderNote);

            // Keep ownership and creation timestamp controlled by API.
            newOrderNote.OrderId = order.Id;
            newOrderNote.CreatedOnUtc = DateTime.UtcNow;

            await _orderNoteApiService.InsertOrderNoteAsync(newOrderNote);

            await CustomerActivityService.InsertActivityAsync("AddNewOrderNote", "A new order note has been created", newOrderNote);

            var orderNotesRootObject = new OrderNotesRootObject();
            orderNotesRootObject.OrderNotes.Add(await _dtoHelper.PrepareOrderNoteDTOAsync(newOrderNote));

            var json = JsonFieldsSerializer.Serialize(orderNotesRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/orders/{orderId}/notes/{orderNoteId}", Name = "UpdateOrderNote")]
        [ProducesResponseType(typeof(OrderNotesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> UpdateOrderNote(
            [FromRoute] int orderId,
            [FromRoute] int orderNoteId,
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<OrderNoteDto>))]
            Delta<OrderNoteDto> orderNoteDelta)
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var order = _orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var orderNoteToUpdate = await _orderNoteApiService.GetOrderNoteByIdAsync(orderNoteId);

            if (orderNoteToUpdate == null || orderNoteToUpdate.OrderId != order.Id)
            {
                return Error(HttpStatusCode.NotFound, "order_note", "not found");
            }

            var parentOrderId = orderNoteToUpdate.OrderId;
            var createdOnUtc = orderNoteToUpdate.CreatedOnUtc;

            orderNoteDelta.Merge(orderNoteToUpdate);

            orderNoteToUpdate.OrderId = parentOrderId;
            orderNoteToUpdate.CreatedOnUtc = createdOnUtc;

            await _orderNoteApiService.UpdateOrderNoteAsync(orderNoteToUpdate);

            await CustomerActivityService.InsertActivityAsync("UpdateOrderNote", "An order note has been updated", orderNoteToUpdate);

            var orderNotesRootObject = new OrderNotesRootObject();
            orderNotesRootObject.OrderNotes.Add(await _dtoHelper.PrepareOrderNoteDTOAsync(orderNoteToUpdate));

            var json = JsonFieldsSerializer.Serialize(orderNotesRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/orders/{orderId}/notes/{orderNoteId}", Name = "DeleteOrderNoteById")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteOrderNoteById([FromRoute] int orderId, [FromRoute] int orderNoteId)
        {
            var order = _orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var orderNote = await _orderNoteApiService.GetOrderNoteByIdAsync(orderNoteId);

            if (orderNote == null || orderNote.OrderId != order.Id)
            {
                return Error(HttpStatusCode.NotFound, "order_note", "not found");
            }

            await _orderNoteApiService.DeleteOrderNoteAsync(orderNote);

            return new RawJsonActionResult("{}");
        }

        [HttpDelete]
        [Route("/api/orders/{orderId}/notes", Name = "DeleteAllOrderNotesForOrder")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteAllOrderNotesForOrder([FromRoute] int orderId)
        {
            var order = _orderApiService.GetOrderById(orderId);

            if (order == null)
            {
                return Error(HttpStatusCode.NotFound, "order", "not found");
            }

            var orderNotes = await _orderNoteApiService.GetAllOrderNotesForOrderAsync(order);

            foreach (var item in orderNotes)
            {
                await _orderNoteApiService.DeleteOrderNoteAsync(item);
            }

            return new RawJsonActionResult("{}");
        }
    }
}
