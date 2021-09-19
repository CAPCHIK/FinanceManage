﻿using FinanceManage.CQRS.Queries;
using FinanceManage.Site.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManage.Site.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeekSpendingController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<WeekSpendingController> logger;

        public WeekSpendingController(
            IMediator mediator,
            ILogger<WeekSpendingController> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }
        [Authorize]
        public async Task<ActionResult<AverageSpending.Result>> GetWeekSpendingAsync([FromQuery] AverageSpending.Command command)
        {
            var userHasAccess = await mediator.Send(
                new GetUserHasAccessToChat.Command(User.GetUserId(), command.ChatId));
            if (!userHasAccess)
            {
                return Forbid();
            }
            return await mediator.Send(command);
        }
    }
}
