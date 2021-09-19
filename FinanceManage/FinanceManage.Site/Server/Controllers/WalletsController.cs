using FinanceManage.CQRS.Commands;
using FinanceManage.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManage.Site.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletsController : ControllerBase
    {
        private readonly IMediator mediator;

        public WalletsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("{chatId:long}")]
        public async Task<ActionResult<List<GetWallets.ResponseObject>>> GetWallets(long chatId)
        {
            var userId = int.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToChat.Command(userId, chatId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(new GetWallets.Query(chatId));
        }

        [HttpPut("{walletId:guid}")]
        public async Task<ActionResult<EditWallet.Result>> UpdateWallet(Guid walletId, [FromBody]EditWallet.Command command)
        {
            var userId = int.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToWallet.Command(userId, walletId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(command with { WalletId = walletId });
        }


        [HttpPost("{chatId:long}")]
        public async Task<ActionResult<CreateWallet.Result>> CreateWallet(
            long chatId,
            [FromBody] CreateWallet.Command command)
        {
            var userId = int.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToChat.Command(userId, chatId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(command with { TelegramChatId = chatId });
        }

        [HttpPost("savebalances/{chatId:long}")]
        public async Task<ActionResult<SaveWalletBalances.Result>> SaveBalances(
            long chatId,
            [FromBody] SaveWalletBalances.Command command)
        {
            var userId = int.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToChat.Command(userId, chatId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(command with { AuthorId = userId, ChatId = chatId });
        }
    }
}
