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

using static FinanceManage.CQRS.Commands.EditWallet;

namespace FinanceManage.CQRS.Handlers.Client.CommandHandlers
{
    internal class EditWalletHandler : BaseClientHandler<Command, Result, EditWalletHandler>
    {
        public EditWalletHandler(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override async Task<Result> InvokeRequest(HttpClient httpClient, Command request, CancellationToken cancellationToken)
        {
            var response = await httpClient.PutAsJsonAsync($"/api/wallets/{request.WalletId}", request, cancellationToken);
            var responseRaw = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<Result>(responseRaw, jsonSerializerOptions);
        }
    }
}
