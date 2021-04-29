using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Handlers.Client.Exceptions
{
    public class ForbidException : ClientMediatorException
    {
        public ForbidException(string message, Exception inner) : base(message, inner) { }
    }
}
