namespace DanialNetFood.Web.Services.DiscountStrategy
{
    public interface IDiscountStrategy
    {
        decimal ApplyDiscount(decimal totalAmount, decimal discountValue);
    }

    public class PercentageDiscountStrategy : IDiscountStrategy
    {
        public decimal ApplyDiscount(decimal totalAmount, decimal discountValue)
        {
            return totalAmount - (totalAmount * (discountValue / 100));
        }
    }

    public class FixedAmountDiscountStrategy : IDiscountStrategy
    {
        public decimal ApplyDiscount(decimal totalAmount, decimal discountValue)
        {
            return Math.Max(0, totalAmount - discountValue);
        }
    }

    public class DiscountContext
    {
        private IDiscountStrategy _strategy;

        public DiscountContext(IDiscountStrategy strategy)
        {
            _strategy = strategy;
        }

        public decimal ExecuteStrategy(decimal totalAmount, decimal discountValue)
        {
            return _strategy.ApplyDiscount(totalAmount, discountValue);
        }
    }
}
