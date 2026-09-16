using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTOs.TaxRates;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class TaxRateDtoMappings
    {
        public static TaxRateDto ToDto(this TaxRate taxRate)
        {
            return taxRate.MapTo<TaxRate, TaxRateDto>();
        }
    }
}
