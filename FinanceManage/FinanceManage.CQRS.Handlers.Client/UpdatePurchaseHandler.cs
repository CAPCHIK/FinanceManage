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

using static FinanceManage.CQRS.Queries.UpdatePurchase;

namespace FinanceManage.CQRS.Handlers.Client
{
    class UpdatePurchaseHandler : IRequestHandler<Command, bool>
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<UpdatePurchaseHandler> logger;

        public UpdatePurchaseHandler(
            HttpClient httpClient,
            ILogger<UpdatePurchaseHandler> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                string requestRow = $"/api/purchases/{request.PurchaseId}";
                var response = await httpClient.PutAsJsonAsync(requestRow, request, cancellationToken: cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return bool.Parse(responseBody);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbidException("Can't get purchases", ex);
            }
        }
    }
}
