using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Models
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public long TelegramChatId { get; set; }

        public List<WalletHistory> History { get; set; }
    }
}
