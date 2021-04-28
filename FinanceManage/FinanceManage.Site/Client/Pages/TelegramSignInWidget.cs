using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceManage.Site.Client.Pages
{
    public class TelegramSignInWidget : ComponentBase
    {
        [Inject]
        public IConfiguration Configuration { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenElement(0, "script");
            builder.AddAttribute(1, "async");
            builder.AddAttribute(2, "src", "https://telegram.org/js/telegram-widget.js?14");
            builder.AddAttribute(2, "data-telegram-login", Configuration.GetValue<string>("TelegramBotOptions:UserName"));
            builder.AddAttribute(3, "data-size", "large");
            builder.AddAttribute(4, "data-userpic", "false");
            builder.AddAttribute(5, "data-onauth", "onTelegramAuth(user)");
            builder.CloseElement();
        }
    }
}

//< script async src = "https://telegram.org/js/telegram-widget.js?14" data-telegram-login="fin_manage_dev_bot" data-size="small" data-userpic="false" data-onauth="onTelegramAuth(user)"></script>

