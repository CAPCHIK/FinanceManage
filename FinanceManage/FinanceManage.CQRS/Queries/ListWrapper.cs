using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Queries
{
    public record ListWrapper<T>(int PageNum, int PageSize, int Total, List<T> Items);
}
