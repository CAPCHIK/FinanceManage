using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class SavePurchase
    {
        public record Command(
          int BuyerTelegramId,
          string Category,
          float Price,
          DateTimeOffset Date,
          long TelegramChatId) : IRequest<bool>;
    }
}
