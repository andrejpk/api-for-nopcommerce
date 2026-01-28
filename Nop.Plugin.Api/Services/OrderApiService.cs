#nullable enable
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public class OrderApiService : IOrderApiService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;

        public OrderApiService(IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository, 
             IRepository<ProductCategory> productCategoryRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        public IList<Order> GetOrdersByCustomerId(int customerId)
        {
            var query = from order in _orderRepository.Table
                        where order.CustomerId == customerId && !order.Deleted
                        orderby order.Id
                        select order;

            return query.ToApiList();
        }

        public IList<Order> GetOrders(
            ISet<int>? ids = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId,
            OrderStatus? status = null, PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null, int? customerId = null,
            int? storeId = null)
        {
            var query = GetOrdersQuery(createdAtMin, createdAtMax, status, paymentStatus, shippingStatus, ids, customerId, storeId);

            if (sinceId > 0)
            {
                query = query.Where(order => order.Id > sinceId);
            }

            return new ApiList<Order>(query, page - 1, limit);
        }

        public Order? GetOrderById(int orderId) => orderId switch
            {
                < 0 => null,
                _ => _orderRepository.Table.FirstOrDefault(order => order.Id == orderId && !order.Deleted)
            };

        public int GetOrdersCount(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, OrderStatus? status = null,
            PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null,
            int? customerId = null, int? storeId = null) =>
            GetOrdersQuery(createdAtMin,
                createdAtMax,
                status,
                paymentStatus,
                shippingStatus,
                customerId: customerId,
                storeId: storeId).Count();

        private IQueryable<Order> GetOrdersQuery(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, OrderStatus? status = null,
            PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null, ISet<int>? ids = null,
            int? customerId = null, int? storeId = null) =>
            _orderRepository.Table
                .Where(order => !order.Deleted)
                .WhereCustomerId(customerId)
                .WhereOrderIdIn(ids)
                .WherePaymentStatus(paymentStatus)
                .WhereShippingStatus(shippingStatus)
                .WhereCreatedAtMin(createdAtMin)
                .WhereCreatedAtMax(createdAtMax)
                .WhereOrderStatus(status)
                .WhereStoreId(storeId)
                .Distinct()
                .OrderBy(order => order.Id);

        public IList<Order> GetOrdersForProductId(int productId, 
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, 
            OrderStatus? status = null, int? storeId = null
            ) =>
            _orderRepository.Table
                .Where(order => !order.Deleted)
                .WhereCreatedAtMin(createdAtMin)
                .WhereCreatedAtMax(createdAtMax)
                .WhereOrderStatus(status)
                .WhereStoreId(storeId)
                .WhereHasProductId(productId, _orderItemRepository)
                .Distinct()
                .OrderBy(order => order.Id)
                .ToApiList();

        public IList<Order> GetOrdersForCategoryId(int categoryId, 
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, 
            OrderStatus? status = null, int? storeId = null) =>
            _orderRepository.Table
                .WhereCreatedAtMin(createdAtMin)
                .WhereCreatedAtMax(createdAtMax)
                .WhereOrderStatus(status)
                .WhereStoreId(storeId)
                .WhereHasCategoryId(categoryId, _orderItemRepository, _productCategoryRepository)
                .Distinct()
                .OrderBy(order => order.Id)
                .ToApiList();
    }
    
    internal static class OrderServiceExtensions
    {
        public static IQueryable<Order> WhereOrderStatus(this IQueryable<Order> order, OrderStatus? status) => status switch
        {
            null => order,
            _ => order.Where(o => o.OrderStatusId == (int)status)
        };
        
        public static IQueryable<Order> WhereShippingStatus(this IQueryable<Order> order, ShippingStatus? status) => status switch
        {
            null => order,
            _ => order.Where(o => o.ShippingStatusId == (int)status)
        };
        
        public static IQueryable<Order> WherePaymentStatus(this IQueryable<Order> order, PaymentStatus? status) => status switch
        {
            null => order,
            _ => order.Where(o => o.PaymentStatusId == (int)status)
        };
        
        public static IQueryable<Order> WhereCreatedAtMin(this IQueryable<Order> order, DateTime? createdAtMin) => createdAtMin switch
        {
            null => order,
            _ => order.Where(o => o.CreatedOnUtc > createdAtMin.Value.ToUniversalTime())
        };
        
        public static IQueryable<Order> WhereCreatedAtMax(this IQueryable<Order> order, DateTime? createdAtMax) => createdAtMax switch
        {
            null => order,
            _ => order.Where(o => o.CreatedOnUtc < createdAtMax.Value.ToUniversalTime())
        };
        
        public static IQueryable<Order> WhereStoreId(this IQueryable<Order> order, int? storeId) => storeId switch
        {
            null => order,
            _ => order.Where(o => o.StoreId == storeId)
        };
        
        public static IQueryable<Order> WhereCustomerId(this IQueryable<Order> order, int? customerId) => customerId switch
        {
            null => order,
            _ => order.Where(o => o.CustomerId == customerId)
        };
        
        public static IQueryable<Order> WhereOrderIdIn(this IQueryable<Order> order, ISet<int>? orderIds) => orderIds switch
        {
            null => order,
            _ => order.Where(o => orderIds.Contains(o.Id))
        };
        
        public static IQueryable<Order> WhereHasProductId(this IQueryable<Order> order, int? productId, 
            IRepository<OrderItem> orderItemRepository) => productId switch
        {
            null => order,
            _ => from o in  order
                join orderItem in orderItemRepository.Table on o.Id equals orderItem.OrderId
                where orderItem.ProductId == productId
                select o
        };
        
        public static IQueryable<Order> WhereHasCategoryId(this IQueryable<Order> order, int? categoryId, 
            IRepository<OrderItem> orderItemRepository, IRepository<ProductCategory> productCategoryRepository) => categoryId switch
        {
            null => order,
            _ => from o in  order
                join orderItem in orderItemRepository.Table on o.Id equals orderItem.OrderId
                join productCategory in productCategoryRepository.Table on orderItem.ProductId equals productCategory.ProductId
                where productCategory.CategoryId == categoryId
                select o
        };
        
        public static ApiList<T>  ToApiList<T>(this IQueryable<T> query, int pageIndex = 0, int pageSize = Constants.Configurations.MaxLimit) => 
             new (query, pageIndex, pageSize);
    }
}
