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
    public class GetUserHasAccessToPurchaseHandler : IRequestHandler<GetUserHasAccessToPurchase.Command, bool>
    {
        private readonly FinanceManageDbContext dbContext;

        public GetUserHasAccessToPurchaseHandler(FinanceManageDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<bool> Handle(GetUserHasAccessToPurchase.Command request, CancellationToken cancellationToken)
        {
            return dbContext.Purchases
                .Where(p => p.BuyerTelegramId == request.UserId)
                .Where(p => p.Id == request.PurchaseId)
                .AnyAsync(cancellationToken: cancellationToken);
        }
    }
}
