namespace DanialNetFood.Web.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Preparing, ReadyForPickup, OutForDelivery, Delivered, Cancelled
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public int? DriverId { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int OrderId { get; set; }
        public List<OrderItemOption> Options { get; set; } = new();
    }
}
