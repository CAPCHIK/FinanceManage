using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Handlers.Client.Exceptions
{

    [Serializable]
    public abstract class ClientMediatorException : Exception
    {
        public ClientMediatorException() { }
        public ClientMediatorException(string message) : base(message) { }
        public ClientMediatorException(string message, Exception inner) : base(message, inner) { }
        protected ClientMediatorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
