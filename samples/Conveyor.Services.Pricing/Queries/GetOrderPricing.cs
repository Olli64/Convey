using System;
using Convey.CQRS.Queries;
using Conveyor.Services.Pricing.DTO;

namespace Conveyor.Services.Pricing.Queries
{
    public class GetOrderPricing : IQuery<PricingDto>
    {
        public Guid OrderId { get; set; }
    }
}
