using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Services
{
    public class OrderNoteApiService : IOrderNoteApiService
    {
        private readonly IOrderService _orderService;
        private readonly IRepository<OrderNote> _orderNoteRepository;

        public OrderNoteApiService(IOrderService orderService, IRepository<OrderNote> orderNoteRepository)
        {
            _orderService = orderService;
            _orderNoteRepository = orderNoteRepository;
        }

        public async Task<OrderNote> GetOrderNoteByIdAsync(int orderNoteId)
        {
            return await _orderService.GetOrderNoteByIdAsync(orderNoteId);
        }

        public async Task<IList<OrderNote>> GetAllOrderNotesForOrderAsync(Order order)
        {
            return await _orderService.GetOrderNotesByOrderIdAsync(order.Id);
        }

        public async Task<IList<OrderNote>> GetOrderNotesForOrderAsync(Order order, int limit, int page, int sinceId)
        {
            var orderNotes = (await _orderService.GetOrderNotesByOrderIdAsync(order.Id)).AsQueryable();

            return new ApiList<OrderNote>(orderNotes, page - 1, limit);
        }

        public async Task<int> GetOrderNotesCountAsync(Order order)
        {
            var orderNotesCount = (await _orderService.GetOrderNotesByOrderIdAsync(order.Id)).Count;

            return orderNotesCount;
        }

        public async Task InsertOrderNoteAsync(OrderNote orderNote)
        {
            await _orderService.InsertOrderNoteAsync(orderNote);
        }

        public async Task UpdateOrderNoteAsync(OrderNote orderNote)
        {
            await _orderNoteRepository.UpdateAsync(orderNote);
        }

        public async Task DeleteOrderNoteAsync(OrderNote orderNote)
        {
            await _orderService.DeleteOrderNoteAsync(orderNote);
        }
    }
}
