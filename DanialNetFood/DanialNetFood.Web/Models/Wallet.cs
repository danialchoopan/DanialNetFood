using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanialNetFood.Web.Models
{
    public class Wallet
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public User User { get; set; } = null!;

        public List<WalletTransaction> Transactions { get; set; } = new();
    }

    public class WalletTransaction
    {
        public int Id { get; set; }
        public int WalletUserId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "Credit"; // Credit, Debit
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
