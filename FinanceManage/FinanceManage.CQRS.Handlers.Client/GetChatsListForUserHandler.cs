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

using static FinanceManage.CQRS.Queries.GetChatsListForUser;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class GetChatsListForUserHandler : IRequestHandler<Command, List<Response>>
    {
        private readonly HttpClient httpClient;

        public GetChatsListForUserHandler(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<List<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<Response>>($"/api/chats", cancellationToken: cancellationToken);
                return response;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbidException("Can't get purchases", ex);
            }
        }
    }
}
