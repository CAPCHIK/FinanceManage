using FinanceManage.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Queries.GetAvailableCategoriesForChat;


namespace FinanceManage.CQRS.Handlers.Server
{
    class GetAvailableCategoriesForChatHandler : IRequestHandler<Command, List<string>>
    {
        private FinanceManageDbContext dbContext;

        public GetAvailableCategoriesForChatHandler(FinanceManageDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var categories = await dbContext
                .Purchases
                .Where(p => p.TelegramChatId == request.ChatId)
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(p => p.Count)
                .Select(p => p.Category)
                .ToListAsync(cancellationToken);
            return categories;
        }
    }
}
