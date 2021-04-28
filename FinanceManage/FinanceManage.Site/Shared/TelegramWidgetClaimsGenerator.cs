using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Site.Shared
{
    public static class TelegramWidgetClaimsGenerator
    { 
        public static ClaimsPrincipal GetPrincipal(TelegramUserInfo userInfo)
        {
            var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userInfo.UserName),
                    new Claim(ClaimTypes.GivenName, userInfo.FirstName),
                    new Claim(ClaimTypes.Surname, userInfo.LastName),
                    new Claim(ClaimTypes.DateOfBirth, userInfo.AuthDate.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, nameof(TelegramWidgetClaimsGenerator));
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
