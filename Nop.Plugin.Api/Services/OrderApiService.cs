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
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;

        public OrderApiService(IRepository<Order> orderRepository, IRepository<OrderItem> orderItemRepository, 
            IRepository<Category> categoryRepository, IRepository<ProductCategory> productCategoryRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _categoryRepository = categoryRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        public IList<Order> GetOrdersByCustomerId(int customerId)
        {
            var query = from order in _orderRepository.Table
                        where order.CustomerId == customerId && !order.Deleted
                        orderby order.Id
                        select order;

            return new ApiList<Order>(query, 0, Constants.Configurations.MaxLimit);
        }

        public IList<Order> GetOrders(
            IList<int> ids = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
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

        public Order GetOrderById(int orderId)
        {
            if (orderId <= 0)
            {
                return null;
            }

            return _orderRepository.Table.FirstOrDefault(order => order.Id == orderId && !order.Deleted);
        }

        public int GetOrdersCount(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, OrderStatus? status = null,
            PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null,
            int? customerId = null, int? storeId = null)
        {
            var query = GetOrdersQuery(createdAtMin, createdAtMax, status, paymentStatus, shippingStatus, customerId: customerId, storeId: storeId);

            return query.Count();
        }

        private IQueryable<Order> GetOrdersQuery(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, OrderStatus? status = null,
            PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null, IList<int> ids = null,
            int? customerId = null, int? storeId = null)
        {
            var query = _orderRepository.Table;

            if (customerId != null)
            {
                query = query.Where(order => order.CustomerId == customerId);
            }

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(c => ids.Contains(c.Id));
            }

            if (status != null)
            {
                query = query.Where(order => order.OrderStatusId == (int)status);
            }

            if (paymentStatus != null)
            {
                query = query.Where(order => order.PaymentStatusId == (int)paymentStatus);
            }

            if (shippingStatus != null)
            {
                query = query.Where(order => order.ShippingStatusId == (int)shippingStatus);
            }

            query = query.Where(order => !order.Deleted);

            if (createdAtMin != null)
            {
                query = query.Where(order => order.CreatedOnUtc > createdAtMin.Value.ToUniversalTime());
            }

            if (createdAtMax != null)
            {
                query = query.Where(order => order.CreatedOnUtc < createdAtMax.Value.ToUniversalTime());
            }

            if (storeId != null)
            {
                query = query.Where(order => order.StoreId == storeId);
            }

            query = query.OrderBy(order => order.Id);

            //query = query.Include(c => c.Customer);
            //query = query.Include(c => c.BillingAddress);
            //query = query.Include(c => c.ShippingAddress);
            //query = query.Include(c => c.PickupAddress);
            //query = query.Include(c => c.RedeemedRewardPointsEntry);
            //query = query.Include(c => c.DiscountUsageHistory);
            //query = query.Include(c => c.GiftCardUsageHistory);
            //query = query.Include(c => c.OrderNotes);
            //query = query.Include(c => c.OrderItems);
            //query = query.Include(c => c.Shipments);

            return query;
        }
        
        public IList<Order> GetOrdersForProductId(int productId, 
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, 
            OrderStatus? status = null, int? storeId = null)
        {
            var query = from orderItem in _orderItemRepository.Table
                join order in _orderRepository.Table on orderItem.OrderId equals order.Id
                where orderItem.ProductId == productId
                select order;
            
            if (createdAtMin != null)
            {
                query = query.Where(order => order.CreatedOnUtc > createdAtMin.Value.ToUniversalTime());
            }

            if (createdAtMax != null)
            {
                query = query.Where(order => order.CreatedOnUtc < createdAtMax.Value.ToUniversalTime());
            }
            
            if (status != null)
            {
                query = query.Where(order => order.OrderStatusId == (int)status);
            }

            if (storeId != null)
            {
                query = query.Where(order => order.StoreId == storeId);
            }
            
            return new ApiList<Order>(query, 0, Constants.Configurations.MaxLimit);
        }
        
        public IList<Order> GetOrdersForCategoryId(int categoryId, 
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, 
            OrderStatus? status = null, int? storeId = null)
        {
            var query = from productCategory in _productCategoryRepository.Table 
                join orderItem in _orderItemRepository.Table on productCategory.ProductId equals orderItem.ProductId
                join order in _orderRepository.Table on orderItem.OrderId equals order.Id
                where productCategory.CategoryId == categoryId
                select order;
            query = query.Distinct();
            
            if (createdAtMin != null)
            {
                query = query.Where(order => order.CreatedOnUtc > createdAtMin.Value.ToUniversalTime());
            }

            if (createdAtMax != null)
            {
                query = query.Where(order => order.CreatedOnUtc < createdAtMax.Value.ToUniversalTime());
            }
            
            if (status != null)
            {
                query = query.Where(order => order.OrderStatusId == (int)status);
            }

            if (storeId != null)
            {
                query = query.Where(order => order.StoreId == storeId);
            }

            return new ApiList<Order>(query, 0, Constants.Configurations.MaxLimit);
        }
    }
}
