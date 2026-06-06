namespace DanialNetFood.Web.Models
{
    public class OrderItemOption
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int FoodOptionId { get; set; }
        public string OptionName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
