using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.TelegramBot.InlineQueryModels
{
    public class InlineQueryBase
    {
        public CallbackQueryCommand Command { get; set; }
        public InlineQueryBase(CallbackQueryCommand command)
        {
            Command = command;
        }
    }
}
