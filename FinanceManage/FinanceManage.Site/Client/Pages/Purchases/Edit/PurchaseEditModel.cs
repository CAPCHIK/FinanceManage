using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceManage.Site.Client.Pages.Purchases.Edit
{
    public class PurchaseEditModel
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        [Range(0.01, float.MaxValue)]
        public float Price { get; set; }
    }
}
