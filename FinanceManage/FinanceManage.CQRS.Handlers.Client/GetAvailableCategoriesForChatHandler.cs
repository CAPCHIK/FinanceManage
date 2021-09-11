using FinanceManage.CQRS.Handlers.Client.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Queries.GetAvailableCategoriesForChat;


namespace FinanceManage.CQRS.Handlers.Client
{
    class GetAvailableCategoriesForChatHandler : IRequestHandler<Command, List<string>>
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<GetAvailableCategoriesForChatHandler> logger;

        public GetAvailableCategoriesForChatHandler(
            HttpClient httpClient,
            ILogger<GetAvailableCategoriesForChatHandler> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        async Task<List<string>> IRequestHandler<Command, List<string>>.Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<string>>($"/api/purchases/categories/{request.ChatId}", cancellationToken: cancellationToken);
                return response;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbidException("Can't get week spending info", ex);
            }
        }
    }
}
