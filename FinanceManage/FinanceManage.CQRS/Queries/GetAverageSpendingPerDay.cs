using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetAverageSpendingPerDay
    {
        public record Command(long ChannelId, DateTimeOffset DayStart, TimeSpan period) : IRequest<float>;
    }
}
