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
using static FinanceManage.CQRS.Queries.WeekSpending;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class WeekSpendingHandler : IRequestHandler<Command, Result>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly ILogger<WeekSpendingHandler> logger;

        public WeekSpendingHandler(
            FinanceManageDbContext dbContext,
            ILogger<WeekSpendingHandler> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        async Task<Result> IRequestHandler<Command, Result>.Handle(Command request, CancellationToken cancellationToken)
        {
            var dateStart = request.WeekStart;
            var dateEnd = request.WeekStart.AddDays(7);

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
