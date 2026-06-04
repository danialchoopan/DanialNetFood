namespace DanialNetFood.Web.Models.ViewModels
{
    public class CartItem
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Total);
    }
}
