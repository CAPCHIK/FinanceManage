using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetAllPurchases
    {
        public record Response(Guid Id, int BuyerTelegramId, string Category, float Price, DateTimeOffset Date);
        public record Command(long ChatId) : IRequest<List<Response>>;
    }
}
