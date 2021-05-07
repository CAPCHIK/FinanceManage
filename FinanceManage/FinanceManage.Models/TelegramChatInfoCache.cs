using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Models
{
    public class TelegramChatInfoCache
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset CachedDate { get; set; }
    }
}
