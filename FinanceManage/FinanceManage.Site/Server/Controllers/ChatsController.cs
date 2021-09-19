using FinanceManage.Site.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.GetChatsListForUser;

namespace FinanceManage.Site.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IMediator mediator;

        public ChatsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Response>>> Get()
        {
            var chatsList = await mediator.Send(new Command(User.GetUserId()));

            return chatsList;
        }
    }
}
