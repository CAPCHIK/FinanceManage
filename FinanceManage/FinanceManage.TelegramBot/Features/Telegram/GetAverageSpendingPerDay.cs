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

namespace FinanceManage.TelegramBot.Features.Telegram
{
    public class GetAverageSpendingPerDay
    {
        public record Command(long ChannelId, DateTimeOffset DayStart, TimeSpan period) : IRequest<float>;

        public class Handler : IRequestHandler<Command, float>
        {
            private readonly FinanceManageDbContext dbContext;
            private readonly ILogger<Handler> logger;

            public Handler(FinanceManageDbContext dbContext, ILogger<Handler> logger)
            {
                this.dbContext = dbContext;
                this.logger = logger;
            }
            public async Task<float> Handle(Command request, CancellationToken cancellationToken)
            {
                var dayStart = request.DayStart.Date;
                var end = dayStart.Add(request.period);
                logger.LogInformation($"start: {dayStart} end: {end}");
                var fromDb = await dbContext.Purchases
                    .Where(p => p.TelegramChannelId == request.ChannelId)
                    .Where(p => p.Date >= dayStart)
                    .Where(p => p.Date < end)
                    .Select(p => new { p.Date, p.Price})
                    .ToListAsync(cancellationToken: cancellationToken);
                return fromDb
                    .GroupBy(p => new { p.Date.Year, p.Date.Month, p.Date.Day })
                    .Select(g => g.Sum(p => p.Price))
                    .DefaultIfEmpty(0f)
                    .Average();
            }
        }
    }
}
