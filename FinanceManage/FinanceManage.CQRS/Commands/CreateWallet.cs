using FinanceManage.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Commands
{
    public class CreateWallet
    {
        public record Command(
            string Title,
            string Description,
            WalletType WalletType,
            long TelegramChatId) : IRequest<Result>;
        public record Result(ErrorReason? Error);
        public enum ErrorReason
        {
            TitleExists,
            UnexpectedError
        }
    }
}
