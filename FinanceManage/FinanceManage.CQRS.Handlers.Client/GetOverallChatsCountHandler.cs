using FinanceManage.CQRS.Handlers.Client.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Queries.GetOverallChatsCount;


namespace FinanceManage.CQRS.Handlers.Client
{
    public class GetOverallChatsCountHandler : IRequestHandler<Command, Response>
    {
        private readonly HttpClient httpClient;

        public GetOverallChatsCountHandler(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                string requestRow = $"/api/admin/getOverallChatsCount?activeChatPeriod={request.ActiveChatPeriod}";
                var response = await httpClient.GetFromJsonAsync<Response>(requestRow, cancellationToken: cancellationToken);
                return response;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbidException("Can't get purchases", ex);
            }
        }
    }
}
