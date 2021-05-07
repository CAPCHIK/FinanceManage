using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManage.CQRS.Notifications
{
    public record UpdateTelegramChatTitle(long ChatId, string FirstName, string LastName, string Title) : INotification;
}
