using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Commands
{
    public class SaveWalletBalances
    {
        public record Command(long ChatId, int AuthorId, DateTimeOffset Date, List<WalletBalance> Balances): IRequest<Result>;
        public record WalletBalance(Guid Id, float Balance);
        
        public record Result(ErrorReason? ErrorReason);
        public enum ErrorReason
        {
            NotAllWalletsFilled,
            DublicateWallets,
            UnexpectedError
        }
    }
}
