using FinanceManage.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Queries.GetWallets;

namespace FinanceManage.CQRS.Handlers.Server
{
    class GetWalletsHandler : IRequestHandler<Query, List<ResponseObject>>
    {
        private readonly FinanceManageDbContext dbContext;

        public GetWalletsHandler(FinanceManageDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<ResponseObject>> Handle(Query request, CancellationToken cancellationToken)
        {
            var wallets = await dbContext
               .Wallets
               .Where(w => w.TelegramChatId == request.TelegramChatId)
               .OrderBy(w => w.Title)
               .Select(w => new ResponseObject(w.Id, w.Title, w.Description, w.WalletType, w.History.Max(wh => wh.Sum)))
               .ToListAsync();
            return wallets;
        }
    }
}
