using FinanceManage.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Queries.GetOverallChatsCount;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class GetOverallChatsCountHandler : IRequestHandler<Command, Response>
    {
        private readonly FinanceManageDbContext dbContext;

        public GetOverallChatsCountHandler(
            FinanceManageDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var results = await dbContext
                .Purchases
                .GroupBy(p => new { p.TelegramChatId })
                .Select(g => g.Max(p => p.Date))
                .ToListAsync(cancellationToken: cancellationToken);
            var latestDateForActive = DateTimeOffset.UtcNow - request.ActiveChatPeriod;
            return new Response(results.Where(r => r > latestDateForActive).Count(), results.Count, request.ActiveChatPeriod.Ticks);
        }
    }
}
