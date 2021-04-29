
using FinanceManage.CQRS.Handlers.Client.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.WeekSpending;

namespace FinanceManage.CQRS.Handlers.Client
{
    public class WeekSpendingHandler : IRequestHandler<Command, Result>
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<WeekSpendingHandler> logger;

        public WeekSpendingHandler(
            HttpClient httpClient,
            ILogger<WeekSpendingHandler> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        async Task<Result> IRequestHandler<Command, Result>.Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<Result>($"/api/weekspending?{request.ConevertToQuery()}", cancellationToken: cancellationToken);
                return response;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbidException("Can't get week spending info", ex);
            }
        }
    }
}
