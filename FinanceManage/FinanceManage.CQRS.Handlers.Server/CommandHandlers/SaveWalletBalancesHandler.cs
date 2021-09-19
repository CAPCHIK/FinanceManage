using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Commands.SaveWalletBalances;

namespace FinanceManage.CQRS.Handlers.Server.CommandHandlers
{
    class SaveWalletBalancesHandler : IRequestHandler<Command, Result>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly ILogger<SaveWalletBalancesHandler> logger;

        public SaveWalletBalancesHandler(
            FinanceManageDbContext dbContext,
            ILogger<SaveWalletBalancesHandler> logger
            )
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var ids = request.Balances.Select(b => b.Id).Distinct().ToList();
            if (ids.Count != request.Balances.Count)
            {
                return new Result(ErrorReason.DublicateWallets);
            }
            var allWallets = await dbContext
                .Wallets
                .AsNoTracking()
                .Where(w => w.TelegramChatId == request.ChatId)
                .ToListAsync(cancellationToken: cancellationToken);
            if (allWallets.Count != ids.Count || ids.Any(id => !allWallets.Any(w => w.Id == id)))
            {
                return new Result(ErrorReason.NotAllWalletsFilled);
            }

            foreach (var balanceRecord in request.Balances)
            {
                var record = new WalletHistory
                {
                    AuthorTelegramId = request.AuthorId,
                    Date = request.Date,
                    WalletId = balanceRecord.Id,
                    Sum = balanceRecord.Balance
                };
                dbContext.WalletHistories.Add(record);
            }
            var saved = await dbContext.SaveChangesAsync();
            if (saved != request.Balances.Count)
            {
                return new Result(ErrorReason.UnexpectedError);
            } else
            {
                return new Result(null);
            }
        }
    }
}
