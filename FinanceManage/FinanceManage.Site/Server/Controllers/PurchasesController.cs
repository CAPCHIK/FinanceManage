using FinanceManage.CQRS.Queries;
using FinanceManage.Site.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public class PurchasesController : ControllerBase
    {
        private readonly IMediator mediator;

        public PurchasesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("{chatId:long}")]
        public async Task<ActionResult<ListWrapper<GetPurchases.Response>>> Get(
            [FromRoute] long chatId,
            [FromQuery] GetPurchases.Command command)
        {
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToChat.Command(User.GetUserId(), chatId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(command with { ChatId = chatId });
        }

        [HttpPost("{chatId:long}")]
        public async Task<ActionResult<bool>> CreatePurchase(
            [FromRoute] long chatId,
            [FromBody] SavePurchase.Command command)
        {
            var userId = User.GetUserId();
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToChat.Command(userId, chatId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(command with { BuyerTelegramId = userId, TelegramChatId = chatId } );
        }

        [HttpPut("{purchaseId:guid}")]
        public async Task<ActionResult<bool>> UpdatePurchase(
            [FromRoute] Guid purchaseId,
            [FromBody] UpdatePurchase.Command command)
        {
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToPurchase.Command(User.GetUserId(), purchaseId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(command with { PurchaseId = purchaseId });
        }

        [HttpGet("categories/{chatId:long}")]
        public async Task<ActionResult<List<string>>> GetCategories(long chatId)
        {
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToChat.Command(User.GetUserId(), chatId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(new GetAvailableCategoriesForChat.Command(chatId));
        }
    }
}
