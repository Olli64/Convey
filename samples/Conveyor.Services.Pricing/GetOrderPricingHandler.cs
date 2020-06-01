using System.Threading.Tasks;
using Convey.CQRS.Queries;
using Conveyor.Services.Pricing.DTO;
using Conveyor.Services.Pricing.Queries;

namespace Conveyor.Services.Pricing
{
    public class GetOrderPricingHandler : IQueryHandler<GetOrderPricing, PricingDto>
    {
        public async Task<PricingDto> HandleAsync(GetOrderPricing query)
        {
            return new PricingDto
            {
                OrderId = query.OrderId,
                TotalAmount = 20.50m
            };
        }
    }
}
