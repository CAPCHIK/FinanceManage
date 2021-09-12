using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetOverallChatsCount
    {
        public record Command(TimeSpan ActiveChatPeriod) : IRequest<Response>;
        public record Response(int ActiveChats, int TotalChats, long ActiveChatPeriodTicks);
    }
}
