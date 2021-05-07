using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetChatsListForUser
    {
        public record Response(long ChatId, string ChatName);
        public record Command(int UserId) : IRequest<List<Response>>;
    }
}
