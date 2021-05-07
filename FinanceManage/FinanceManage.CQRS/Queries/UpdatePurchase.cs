using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public class UpdatePurchase
    {
        public record Command(
            Guid PurchaseId,
            [Required]
            string Category,
            [Required]
            float Price,
            [Required]
            DateTimeOffset Date) : IRequest<bool>;
    }
}
