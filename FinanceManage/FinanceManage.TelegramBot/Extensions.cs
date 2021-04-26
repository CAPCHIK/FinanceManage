using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FinanceManage.TelegramBot
{
    static class Extensions
    {
        private static readonly NumberFormatInfo nfi;

        static Extensions()
        {
            nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
        }
        public static string ToMoneyString(this float value)
        {
            return value.ToString("#,0.00", nfi);
        }
        public static string CompactMarkdownV2Date(this DateTimeOffset dateTime)
        {
            return dateTime.ToString("dd.MM").Replace(".", "\\.");
        }
        private static readonly Regex escapeAsMarkdownV2Regex = new(@"_|\*|\[|\]|\(|\)|~|`|>|#|\+|-|=|\\|{|}|\.|!");
        public static string EscapeAsMarkdownV2(this string input)
        {
            return escapeAsMarkdownV2Regex.Replace(input, m => $"\\{m.Value}");

        }
    }
}
