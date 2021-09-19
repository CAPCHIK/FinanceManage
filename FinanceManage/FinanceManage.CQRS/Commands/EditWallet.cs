using FinanceManage.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Commands
{
    public class EditWallet
    {
        public record Command(Guid WalletId, string Title, string Description, WalletType WalletType): IRequest<Result>;
        public record Result(ErrorReason? Error);
        public enum ErrorReason
        {
            NotFound,
            TitleExists,
            UnexpectedError
        }
    }
}
