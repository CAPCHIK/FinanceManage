using FinanceManage.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetWallets
    {
        public record Query(long TelegramChatId) : IRequest<List<ResponseObject>>;
        public record ResponseObject(Guid Id, string Title, string Description, WalletType WalletType, float? Balance);
    }
}
