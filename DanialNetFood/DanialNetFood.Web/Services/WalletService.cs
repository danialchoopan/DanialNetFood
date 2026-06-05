using DanialNetFood.Web.Data.UnitOfWork;
using DanialNetFood.Web.Models;

namespace DanialNetFood.Web.Services
{
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync(int userId);
        Task CreditAsync(int userId, decimal amount, string description);
        Task DebitAsync(int userId, decimal amount, string description);
    }

    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(userId);
            return wallet?.Balance ?? 0;
        }

        public async Task CreditAsync(int userId, decimal amount, string description)
        {
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(userId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 0 };
                await _unitOfWork.Wallets.AddAsync(wallet);
            }

            wallet.Balance += amount;
            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletUserId = userId,
                Amount = amount,
                Type = "Credit",
                Description = description
            });
        }

        public async Task DebitAsync(int userId, decimal amount, string description)
        {
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(userId);
            if (wallet == null || wallet.Balance < amount)
                throw new Exception("اعتبار کیف پول کافی نیست.");

            wallet.Balance -= amount;
            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletUserId = userId,
                Amount = amount,
                Type = "Debit",
                Description = description
            });
        }
    }
}
