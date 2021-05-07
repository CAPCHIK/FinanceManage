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
        public static string ConvertToQuery<TV>(this TV request, params string[] ignoreFields) where TV : IBaseRequest
        {
            ignoreFields = ignoreFields ?? throw new ArgumentNullException(nameof(ignoreFields));
            request = request ?? throw new ArgumentNullException(nameof(request));
            return string.Join('&', JsonSerializer.Deserialize<JsonDocument>(JsonSerializer.Serialize(request))
                .RootElement.EnumerateObject()
                .Where(p => !ignoreFields.Contains(p.Name))
                .Select(p => (name: HttpUtility.UrlEncode(p.Name), value: HttpUtility.UrlEncode(p.Value.ToString())))
                .Select(p => $"{p.name}={p.value}"));
        }
    }
}
