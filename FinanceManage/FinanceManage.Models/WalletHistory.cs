using System;

namespace FinanceManage.Models
{
    public class WalletHistory
    {
        public int Id { get; set; }

        public DateTimeOffset Date { get; set; }
        public float Sum { get; set; }
        public int AuthorTelegramId { get; set; }


        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; }
    }
}