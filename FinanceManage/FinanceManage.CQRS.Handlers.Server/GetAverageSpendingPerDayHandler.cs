using FinanceManage.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.GetAverageSpendingPerDay;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class GetAverageSpendingPerDayHandler : IRequestHandler<Command, float>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly ILogger<GetAverageSpendingPerDayHandler> logger;

        public GetAverageSpendingPerDayHandler(FinanceManageDbContext dbContext, ILogger<GetAverageSpendingPerDayHandler> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }
        public async Task<float> Handle(Command request, CancellationToken cancellationToken)
        {
            var dayStart = request.DayStart.Date;
            var end = dayStart.Add(request.period);
            var daysCount = end - dayStart;
            logger.LogDebug($"start: {dayStart} end: {end} days count: {daysCount.Days}");
            var fromDb = await dbContext.Purchases
                .Where(p => p.TelegramChannelId == request.ChannelId)
                .Where(p => p.Date >= dayStart)
                .Where(p => p.Date < end)
                .Select(p => new { p.Date, p.Price })
                .ToListAsync(cancellationToken: cancellationToken);
            return fromDb
                .GroupBy(p => new { p.Date.Year, p.Date.Month, p.Date.Day })
                .Select(g => g.Sum(p => p.Price))
                .DefaultIfEmpty(0f)
                .Sum() / daysCount.Days;
        }
    }
}
