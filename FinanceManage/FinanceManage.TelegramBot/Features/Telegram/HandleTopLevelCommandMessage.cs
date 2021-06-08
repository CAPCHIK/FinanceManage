using FinanceManage.CQRS.Queries;
using FinanceManage.Models.ServerSide.Options;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinanceManage.TelegramBot.Features.Telegram
{
    public class HandleTopLevelCommandMessage
    {
        public record Command(Message Message) : IRequest<bool>;


        private const string SpendingStatistic = "Статистика трат";
        private const string AllPurchasesGraph = "Граф со всеми покупками";
        public static readonly IReadOnlyCollection<string> TopLevelCommands = new List<string>
        {
            SpendingStatistic,
            AllPurchasesGraph
        };

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly IOptions<TelegramBotOptions> options;
            private readonly ILogger<Handler> logger;
            private readonly IMediator mediator;

            public Handler(IOptions<TelegramBotOptions> options, ILogger<Handler> logger, IMediator mediator)
            {
                this.options = options;
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
                        var (text, markup) = await mediator.Send(new PrepareWeekSpendingMessage.Command(message.Date.Date.AddDays(-6), message.Chat.Id, AverageSpending.CategoryMode.Compact));
                        await mediator.Send(new SendMessage.Command(message.Chat.Id, text, message.MessageId, ParseMode.MarkdownV2, markup));
                        break;
                    case AllPurchasesGraph:
                        var targetUrl = new UriBuilder(options.Value.SiteBaseAddres)
                        {
                            Path = $"purchases/{message.Chat.Id}"
                        }.Uri.ToString();
                        var inlineMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Посмотреть граф", targetUrl));
                        await mediator.Send(new SendMessage.Command(message.Chat.Id, "Перейдите по ссылке для просмотра графа", message.MessageId, ParseMode.MarkdownV2, inlineMarkup));
                        break;
                    default:
                        logger.LogError($"Command {message.Text} is not supported");
                        break;
                }
            }
        }


    }
}
