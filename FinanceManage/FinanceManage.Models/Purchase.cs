using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Models
{
    public class Purchase
    {
        public Guid Id { get; set; }
        public int BuyerTelegramId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public DateTimeOffset Date { get; set; }
        public long TelegramChatId { get; set; }
    }
}
