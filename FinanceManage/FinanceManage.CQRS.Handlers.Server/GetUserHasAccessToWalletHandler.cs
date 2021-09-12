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
    public class GetUserHasAccessToWalletHandler : IRequestHandler<GetUserHasAccessToWallet.Command, bool>
    {
        private readonly FinanceManageDbContext dbContext;

        public GetUserHasAccessToWalletHandler(FinanceManageDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> Handle(GetUserHasAccessToWallet.Command request, CancellationToken cancellationToken)
        {
            // TODO create enttity for chat

            var allChatsIds = await dbContext.Purchases
                .Where(p => p.BuyerTelegramId == request.UserId)
                .Select(p => p.TelegramChatId)
                .Distinct()
                .ToListAsync(cancellationToken: cancellationToken);


            return await dbContext.Wallets
                .Where(w => allChatsIds.Contains(w.TelegramChatId))
                .Where(w => w.Id == request.WalletId)
                .AnyAsync(cancellationToken: cancellationToken);
        }
    }
}
