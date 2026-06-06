namespace DanialNetFood.Web.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount => Items.Sum(i => i.Total);
        public decimal DiscountAmount { get; set; }
        public string? DiscountCode { get; set; }
        public decimal FinalAmount => TotalAmount - DiscountAmount;
    }

    public class CartItem
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<CartItemOption> Options { get; set; } = new List<CartItemOption>();
        public decimal Total => (Price + Options.Sum(o => o.Price)) * Quantity;

        public string GetOptionsHash()
        {
            return string.Join(",", Options.Select(o => o.Id).OrderBy(id => id));
        }
    }

    public class CartItemOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
