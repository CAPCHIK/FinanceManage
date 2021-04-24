using FinanceManage.TelegramBot.InlineQueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinanceManage.TelegramBot
{
    public static class JsonOptions
    {
        public static Lazy<JsonSerializerOptions> InlineJeyboardOptions { get; } = new(() =>
         {
             var options = new JsonSerializerOptions();
             options.Converters.Add(new DateTimeOffsetConverter());
             return options;
         });
    }
}
