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
    }
    public class InlineQueryData<T> : InlineQueryBase
    {
        public InlineQueryData(CallbackQueryCommand command, T data)
        {
            Command = command;
            Data = data;
        }
        public T Data { get; set; }
    }
}
