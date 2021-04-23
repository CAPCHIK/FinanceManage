﻿using FinanceManage.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceManage.TelegramBot.Features
{
    public class WeekSpending
    {
        public record Command(DateTimeOffset WeekStart, long channelId) : IRequest<Result>;
        public record Result(float Sum, DateTimeOffset From, DateTimeOffset To, Dictionary<string, float> ByCategory);

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
                    .Where(p => p.TelegramChannelId == request.channelId
                             && p.Date >= dateStart
                             && p.Date <= dateEnd)
                    .GroupBy(p => p.Category)
                    .Select(g => new { Caterory = g.Key, Sum = g.Sum(p => p.Price) })
                    .ToListAsync(cancellationToken: cancellationToken);
                return new Result(
                    response.Select(r => r.Sum).DefaultIfEmpty(0).Sum(),
                    dateStart,
                    dateEnd,
                    response.ToDictionary(r => r.Caterory, r => r.Sum));
            }
        }
    }
}
