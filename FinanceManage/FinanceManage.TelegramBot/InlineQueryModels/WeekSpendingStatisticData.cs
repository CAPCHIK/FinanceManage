﻿using FinanceManage.CQRS.Queries;
using FinanceManage.TelegramBot.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.TelegramBot.InlineQueryModels
{
    public class WeekSpendingStatisticData : InlineQueryBase
    {
        public DateTimeOffset WeekStart { get; set; }
        public AverageSpending.CategoryMode Category { get; set; }

        /// <summary>
        /// Only for json serialize
        /// </summary>
        public WeekSpendingStatisticData() : this(default, default)
        {

        }
        public WeekSpendingStatisticData(DateTimeOffset weekStart, AverageSpending.CategoryMode category) 
            : base(CallbackQueryCommand.WeekSpendingStatistic)
        {
            WeekStart = weekStart;
            Category = category;
        }
    }
}
