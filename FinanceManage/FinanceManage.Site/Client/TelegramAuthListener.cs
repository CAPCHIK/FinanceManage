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
            Console.WriteLine("auth done");
            UserAuthenticated?.Invoke(user);
        }

        public class TelegramUserInfo
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("first_name")]
            public string FirstName { get; set; }
            [JsonPropertyName("last_name")]
            public string LastName { get; set; }
            [JsonPropertyName("username")]
            public string UserName { get; set; }
            [JsonPropertyName("photo_url")]
            public string PhotoUrl { get; set; }
            [JsonPropertyName("auth_date")]
            public int AuthDate { get; set; }
            [JsonPropertyName("hash")]
            public string Hash { get; set; }
        }
    }
}
