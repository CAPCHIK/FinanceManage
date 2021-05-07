﻿using FinanceManage.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.GetPurchases;

namespace FinanceManage.Site.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly IMediator mediator;

        public PurchasesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Authorize]
        [HttpGet("{chatId:long}")]
        public async Task<ActionResult<ListWrapper<Response>>> Get(
            [FromRoute] long chatId,
            [FromQuery] Command command)
        {
            var userHasAccess = await mediator.Send(
               new GetUserHasAccessToChat.Command(int.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value), chatId));

            if (!userHasAccess)
            {
                return Forbid();
            }

            return await mediator.Send(command with { ChatId = chatId });
        }
    }
}
