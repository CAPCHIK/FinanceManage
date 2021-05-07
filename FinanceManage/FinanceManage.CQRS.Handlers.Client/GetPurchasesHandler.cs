using FinanceManage.CQRS.Handlers.Client;
using FinanceManage.CQRS.Handlers.Client.Exceptions;
using FinanceManage.CQRS.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.GetPurchases;

namespace FinanceManage.CQRS.Handlers.Server
{


    public class GetPurchasesHandler : IRequestHandler<Command, ListWrapper<Response>>
    {
        private readonly HttpClient httpClient;

        public GetPurchasesHandler(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<ListWrapper<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                string requestRow = $"/api/purchases/{request.ChatId}?{request.ConvertToQuery(ignoreFields: nameof(request.ChatId))}";
                var response = await httpClient.GetFromJsonAsync<ListWrapper<Response>>(requestRow, cancellationToken: cancellationToken);
                return response;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbidException("Can't get purchases", ex);
            }
        }
    }
}
