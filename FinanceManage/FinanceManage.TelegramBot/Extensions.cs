using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.TelegramBot
{
    static class Extensions
    {
        private static NumberFormatInfo nfi;

        static Extensions()
        {
            nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
        }
        public static string ToMoneyString(this float value)
        {
            return value.ToString("#,0.00", nfi);
        }
    }
}
