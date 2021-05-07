using FinanceManage.CQRS.Queries;
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
        public async Task<ActionResult<WeekSpending.Result>> GetWeekSpendingAsync([FromQuery] WeekSpending.Command command)
        {
            var userHasAccess = await mediator.Send(
                new GetUserHasAccessToChat.Command(int.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value), command.ChatId));
            if (!userHasAccess)
            {
                return Forbid();
            }
            return await mediator.Send(command);
        }
    }
}
