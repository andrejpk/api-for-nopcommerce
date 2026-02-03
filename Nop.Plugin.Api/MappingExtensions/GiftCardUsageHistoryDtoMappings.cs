using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.GiftCards;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class GiftCardUsageHistoryDtoMappings
    {
        public static GiftCardUsageHistoryDto ToDto(this GiftCardUsageHistory giftCardUsageHistory)
        {
            return giftCardUsageHistory.MapTo<GiftCardUsageHistory, GiftCardUsageHistoryDto>();
        }
    }
}
