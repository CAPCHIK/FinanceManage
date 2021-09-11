using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.Models.ServerSide.Options
{
    public class AdministrationOptions
    {
        public AdminRecord[] Admins { get; set; }
    }
    public class AdminRecord
    {
        /// <summary>
        /// Telegram user id
        /// </summary>
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// Some comment for configuration
        /// </summary>
        public string Comment { get; set; }
    }
}
