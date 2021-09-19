using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Queries.GetWallets;

namespace FinanceManage.CQRS.Handlers.Client.QueryHandlers
{
    class GetWalletsHandler : BaseClientHandler<Query, List<ResponseObject>, GetWalletsHandler>
    {
        public GetWalletsHandler(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override Task<List<ResponseObject>> InvokeRequest(HttpClient httpClient, Query request, CancellationToken cancellationToken)
        {
            return httpClient.GetFromJsonAsync<List<ResponseObject>>($"/api/wallets/{request.TelegramChatId}");
        }
    }
}
