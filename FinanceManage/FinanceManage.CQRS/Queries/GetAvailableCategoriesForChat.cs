using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class GetAvailableCategoriesForChat
    {
        public record Command(long ChatId) : IRequest<List<string>>;
    }
}
