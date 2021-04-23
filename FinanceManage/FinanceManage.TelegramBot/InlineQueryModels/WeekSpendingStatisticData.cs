using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.TelegramBot.InlineQueryModels
{
    public class WeekSpendingStatisticData : InlineQueryData<DateTimeOffset>
    {
        /// <summary>
        /// Only for json serialize
        /// </summary>
        public WeekSpendingStatisticData() : this(default)
        {

        }
        public WeekSpendingStatisticData(DateTimeOffset startDate) : base(CallbackQueryCommand.WeekSpendingStatistic, startDate)
        {
        }
    }
}
