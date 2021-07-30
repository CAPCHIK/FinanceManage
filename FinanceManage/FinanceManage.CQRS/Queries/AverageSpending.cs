using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class AverageSpending
    {
        public enum CategoryMode { Compact, Complete }
        public record Command(DateTimeOffset DateFrom, DateTimeOffset DateTo, long ChatId, CategoryMode CategoryMode = CategoryMode.Complete) : IRequest<Result>;
        public record Result(float Sum, DateTimeOffset From, DateTimeOffset To, Dictionary<string, float> ByCategory);
    }
}
