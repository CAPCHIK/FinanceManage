using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Models.ServerSide.Options
{
    public class TelegramBotOptions
    {
        /// <summary>
        /// Bot access token from @BotFather
        /// </summary>
        [Required]
        public string AccessToken { get; set; }
        /// <summary>
        /// Site base address with schema
        /// </summary>
        [Required]
        public string SiteBaseAddres { get; set; }
    }
}
