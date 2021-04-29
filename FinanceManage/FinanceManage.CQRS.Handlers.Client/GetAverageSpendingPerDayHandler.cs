using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.GetAverageSpendingPerDay;

namespace FinanceManage.CQRS.Handlers.Client
{
    //public class GetAverageSpendingPerDayHandler : IRequestHandler<Command, float>
    //{
    //    private readonly ILogger<GetAverageSpendingPerDayHandler> logger;
    //    private readonly HttpClient httpClient;

    //    public GetAverageSpendingPerDayHandler(HttpClient httpClient)
    //    {
    //        this.httpClient = httpClient;
    //    }
    //    public async Task<float> Handle(Command request, CancellationToken cancellationToken)
    //    {

    //    }
    //}
}
