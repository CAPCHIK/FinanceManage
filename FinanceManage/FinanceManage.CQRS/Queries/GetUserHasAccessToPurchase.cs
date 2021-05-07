using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetUserHasAccessToPurchase
    {
        public record Command(int UserId, Guid PurchaseId) : IRequest<bool>;
    }
}
