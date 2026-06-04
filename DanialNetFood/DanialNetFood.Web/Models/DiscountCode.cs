namespace DanialNetFood.Web.Models
{
    public class DiscountCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "Percentage"; // Percentage or FixedAmount
        public decimal Value { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
