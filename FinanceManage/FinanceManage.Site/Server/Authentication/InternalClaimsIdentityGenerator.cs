using FinanceManage.Database;
using FinanceManage.Models.ServerSide.Options;
using FinanceManage.Site.Shared;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceManage.Site.Server.Authentication
{
    public class InternalClaimsIdentityGenerator
    {
        private readonly IOptions<AdministrationOptions> options;
        public InternalClaimsIdentityGenerator(IOptions<AdministrationOptions> options)
        {
            this.options = options;
        }

        public ClaimsIdentity Generate(TelegramUserInfo userInfo)
        {
            var isAdmin = options.Value.Admins?.Any(a => a.Id == userInfo.Id) == true;
            var claims = isAdmin ? new Claim[] {
                new Claim(InternalClaimConstants.SYSTEM_ADMIN_CLAIM_TYPE, "")
            }
            : Enumerable.Empty<Claim>();

            return new ClaimsIdentity(claims, InternalClaimConstants.IDENTITY_AUTH_TYPE);
        }
    }
}
