using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Api.Services
{
    public interface IOrderNoteApiService
    {
        Task<OrderNote> GetOrderNoteByIdAsync(int orderNoteId);
        Task<IList<OrderNote>> GetAllOrderNotesForOrderAsync(Order order);
        Task<IList<OrderNote>> GetOrderNotesForOrderAsync(Order order, int limit, int page, int sinceId);
        Task<int> GetOrderNotesCountAsync(Order order);
        Task InsertOrderNoteAsync(OrderNote orderNote);
        Task UpdateOrderNoteAsync(OrderNote orderNote);
        Task DeleteOrderNoteAsync(OrderNote orderNote);
    }
}
