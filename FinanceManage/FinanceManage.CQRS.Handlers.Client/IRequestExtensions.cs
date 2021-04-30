using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace FinanceManage.CQRS.Handlers.Client
{
    internal static class IRequestExtensions
    {
        public static string ConevertToQuery<TV, TR>(this TV request) where TV : IRequest<TR>
        {
            request = request ?? throw new ArgumentNullException(nameof(request));
            return string.Join('&', JsonSerializer.Deserialize<JsonDocument>(JsonSerializer.Serialize(request))
                .RootElement.EnumerateObject()
                .Select(p => (name: HttpUtility.UrlEncode(p.Name), value: HttpUtility.UrlEncode(p.Value.ToString())))
                .Select(p => $"{p.name}={p.value}"));
        }
    }
}
