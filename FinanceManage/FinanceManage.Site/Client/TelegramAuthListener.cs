using FinanceManage.Site.Shared;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinanceManage.Site.Client
{
    public class TelegramAuthListener
    {
        public static event Action<TelegramUserInfo> UserAuthenticated;
        [JSInvokable]
        public static void UserAuthCallback(TelegramUserInfo user)
        {
            UserAuthenticated?.Invoke(user);
        }
    }
}
