using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Site.Shared
{
    public static class TelegramWidgetClaimsIdentityGenerator
    { 
        public static ClaimsIdentity GetIdentityForUserInfo(TelegramUserInfo userInfo)
        {
            var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userInfo.UserName),
                    new Claim(ClaimTypes.GivenName, userInfo.FirstName),
                    new Claim(ClaimTypes.Surname, userInfo.LastName),
                    new Claim(ClaimTypes.DateOfBirth, userInfo.AuthDate.ToString())
            };

            return new ClaimsIdentity(claims, nameof(TelegramWidgetClaimsIdentityGenerator));
        }
    }
}
