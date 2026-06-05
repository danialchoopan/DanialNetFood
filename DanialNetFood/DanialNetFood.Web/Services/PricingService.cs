using DanialNetFood.Web.Models;

namespace DanialNetFood.Web.Services
{
    public interface IPricingService
    {
        decimal CalculateOrderTotal(Order order);
        decimal CalculateCommission(decimal total);
    }

    public class PricingService : IPricingService
    {
        private const decimal CommissionRate = 0.15m;

        public decimal CalculateOrderTotal(Order order)
        {
            decimal total = 0;
            foreach (var item in order.Items)
            {
                decimal itemTotal = item.Price;
                foreach (var option in item.Options)
                {
                    itemTotal += option.Price;
                }
                total += itemTotal * item.Quantity;
            }
            return total;
        }

        public decimal CalculateCommission(decimal total)
        {
            return total * CommissionRate;
        }
    }
}
