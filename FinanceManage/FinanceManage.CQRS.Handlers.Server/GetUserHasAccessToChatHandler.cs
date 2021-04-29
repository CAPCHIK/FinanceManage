using FinanceManage.CQRS.Queries;
using FinanceManage.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class GetUserHasAccessToChatHandler : IRequestHandler<GetUserHasAccessToChat.Command, bool>
    {
        private readonly FinanceManageDbContext dbContext;

        public GetUserHasAccessToChatHandler(FinanceManageDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<bool> Handle(GetUserHasAccessToChat.Command request, CancellationToken cancellationToken)
        {
            return dbContext.Purchases
                .Where(p => p.BuyerTelegramId == request.UserId)
                .Where(p => p.TelegramChannelId == request.ChatId)
                .AnyAsync(cancellationToken: cancellationToken);
        }
    }
}
