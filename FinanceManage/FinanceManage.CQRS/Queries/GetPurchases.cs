using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetPurchases
    {
        public record Response(Guid Id, int BuyerTelegramId, string Category, float Price, DateTimeOffset Date);
        public record Command(long ChatId, int PageNum = 0, int PageSize = 10, Ordering Ordering = Ordering.OldToNew, string SearchToken= null) : IRequest<ListWrapper<Response>>;
        public enum Ordering { OldToNew, NewToOld }
    }
}
