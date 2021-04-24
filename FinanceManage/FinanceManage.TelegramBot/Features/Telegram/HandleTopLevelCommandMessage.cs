using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FinanceManage.TelegramBot.Features.Telegram
{
    public class HandleTopLevelCommandMessage
    {
        public record Command(Message Message) : IRequest<bool>;


        private const string SpendingStatistic = "Статистика трат";
        public static readonly IReadOnlyCollection<string> TopLevelCommands = new List<string>
        {
            SpendingStatistic
        };

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly ILogger<Handler> logger;
            private readonly IMediator mediator;

            public Handler(ILogger<Handler> logger, IMediator mediator)
            {
                this.logger = logger;
                this.mediator = mediator;
            }
            async Task<bool> IRequestHandler<Command, bool>.Handle(Command request, CancellationToken cancellationToken)
            {

                if (TopLevelCommands.Contains(request.Message.Text))
                {
                    await HandleTopLevelCommand(request.Message, mediator);
                    return true;
                }

                return false;
            }

            private async Task HandleTopLevelCommand(Message message, IMediator mediator)
            {
                switch (message.Text)
                {
                    case SpendingStatistic:
                        var (text, markup) = await mediator.Send(new PrepareWeekSpendingMessage.Command(message.Date.Date.AddDays(-6), message.Chat.Id, WeekSpending.CategoryMode.Compact));
                        await mediator.Send(new SendMessage.Command(message.Chat.Id, text, message.MessageId, ParseMode.MarkdownV2, markup));
                        break;
                    default:
                        logger.LogError($"Command {message.Text} is not supported");
                        break;
                }
            }
        }


    }
}
