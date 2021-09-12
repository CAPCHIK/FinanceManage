using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Site.Shared
{
    public static class AuthorizationOptionsExtensions
    {
        public static void AddFinanceManagePolicies(this AuthorizationOptions options)
        {
            options.AddPolicy("Admin", builder =>
            {
                builder.RequireClaim(InternalClaimConstants.SYSTEM_ADMIN_CLAIM_TYPE);
            });
        }
    }
}
