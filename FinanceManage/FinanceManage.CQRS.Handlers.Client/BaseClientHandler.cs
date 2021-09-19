using FinanceManage.CQRS.Handlers.Client.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Handlers.Client
{
    internal abstract class BaseClientHandler<TRequest, TResponse, THandler> : IRequestHandler<TRequest, TResponse>
        where THandler : BaseClientHandler<TRequest, TResponse, THandler>
        where TRequest : IRequest<TResponse>
    {
        private readonly HttpClient httpClient;

        protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        public BaseClientHandler(
            HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        protected abstract Task<TResponse> InvokeRequest(HttpClient httpClient, TRequest request, CancellationToken cancellationToken);

        async Task<TResponse> IRequestHandler<TRequest, TResponse>.Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await InvokeRequest(httpClient, request, cancellationToken);
                return response;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new ForbidException($"Can't invoke action on type {typeof(THandler).Name}", ex);
            }
        }
    }
}
