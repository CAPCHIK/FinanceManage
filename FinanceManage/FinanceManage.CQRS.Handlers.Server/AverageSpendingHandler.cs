using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.AverageSpending;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class AverageSpendingHandler : IRequestHandler<Command, Result>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly ILogger<AverageSpendingHandler> logger;

        public AverageSpendingHandler(
            FinanceManageDbContext dbContext,
            ILogger<AverageSpendingHandler> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        async Task<Result> IRequestHandler<Command, Result>.Handle(Command request, CancellationToken cancellationToken)
        {
            var dateStart = request.DateFrom;
            var dateEnd = request.DateFrom.AddDays(request.DaysCount);

            var response = await dbContext.Purchases
                .Where(p => p.TelegramChatId == request.ChatId
                         && p.Date >= dateStart
                         && p.Date <= dateEnd)
                .GroupBy(p => p.Category)
                .Select(g => new { Caterory = g.Key, Sum = g.Sum(p => p.Price) })
                .ToListAsync(cancellationToken: cancellationToken);

            if (request.CategoryMode == CategoryMode.Compact)
            {
                response = response
                    .GroupBy(r => r.Caterory.Split(' ').First())
                    .Select(g => new { Caterory = g.Key, Sum = g.Sum(p => p.Sum) })
                    .ToList();
            }

            return new Result(
                response.Select(r => r.Sum).DefaultIfEmpty(0).Sum(),
                dateStart,
                dateEnd,
                response.ToDictionary(r => r.Caterory, r => r.Sum));
        }
    }
}
