using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FinanceManage.CQRS.Handlers.Client
{
    internal static class IRequestExtensions
    {
        public static string ConevertToQuery<T>(this IRequest<T> request)
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            return string.Join('&', request.GetType()
                .GetProperties()
                .Select(p => (name: p.Name, value: p.GetValue(request)))
                .Select(p => (name: HttpUtility.UrlEncode(p.name), value: p.value == null ? null : HttpUtility.UrlEncode(p.value.ToString())))
                .Select(p => $"{p.name}={p.value}"));
        }
    }
}
