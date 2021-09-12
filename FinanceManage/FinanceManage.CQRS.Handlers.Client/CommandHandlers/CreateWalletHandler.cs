using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Commands.CreateWallet;

namespace FinanceManage.CQRS.Handlers.Client.CommandHandlers
{
    internal class CreateWalletHandler : BaseClientHandler<Command, Result, CreateWalletHandler>
    {
        public CreateWalletHandler(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override async Task<Result> InvokeRequest(HttpClient httpClient, Command request, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsJsonAsync($"/api/wallets/{request.TelegramChatId}", request, cancellationToken);
            var responseRaw = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Result>(responseRaw, jsonSerializerOptions);
        }
    }
}
