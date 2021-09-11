using FinanceManage.Site.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceManage.Site.Server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> logger;

        public AuthController(ILogger<AuthController> logger)
        {
            this.logger = logger;
        }
        [Authorize]
        [HttpPost]
        public ActionResult<ClaimModel[]> CheckInternalClaims ()
        {
            return User
                .Identities
                .Single(i => i.AuthenticationType == InternalClaimConstants.IDENTITY_AUTH_TYPE)
                .Claims
                .Select(c => new ClaimModel(c.Type, c.Value))
                .ToArray();
        }
    }
}
