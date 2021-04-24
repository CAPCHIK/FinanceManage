using FinanceManage.TelegramBot.InlineQueryModels;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FinanceManage.TelegramBot.Features.Telegram
{
    public class HandleCallbackQuery
    {
        public record Command(CallbackQuery CallbackQuery) : IRequest;

        public class Handler : IRequestHandler<Command>
        {
            private readonly IMediator mediator;
            private readonly ITelegramBotClient telegramBotClient;
            private readonly ILogger<Handler> logger;

            public Handler(IMediator mediator, ITelegramBotClient telegramBotClient, ILogger<Handler> logger)
            {
                this.mediator = mediator;
                this.telegramBotClient = telegramBotClient;
                this.logger = logger;
            }
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                InlineQueryBase data;
                try
                {
                    data = JsonSerializer.Deserialize<InlineQueryBase>(request.CallbackQuery.Data, JsonOptions.InlineJeyboardOptions.Value);
                }
                catch
                {
                    await mediator.Send(new SendMessage.Command(request.CallbackQuery.Message.Chat.Id, "Данная кнопка не поддерживается", request.CallbackQuery.Message.MessageId), cancellationToken);
                    return default;
                }
                switch (data.Command)
                {
                    case CallbackQueryCommand.WeekSpendingStatistic:
                        logger.LogInformation(request.CallbackQuery.Data);
                        var weekSpendingStatisticData = JsonSerializer.Deserialize<WeekSpendingStatisticData>(request.CallbackQuery.Data, JsonOptions.InlineJeyboardOptions.Value);
                        var (text, markup) = await mediator.Send(new PrepareWeekSpendingMessage.Command(weekSpendingStatisticData.WeekStart, request.CallbackQuery.Message.Chat.Id, weekSpendingStatisticData.Category), cancellationToken);
                        await telegramBotClient.EditMessageTextAsync(request.CallbackQuery.Message.Chat.Id, request.CallbackQuery.Message.MessageId, text, ParseMode.MarkdownV2, replyMarkup: markup, cancellationToken: cancellationToken);
                        break;
                    default:
                        await mediator.Send(new SendMessage.Command(request.CallbackQuery.Message.Chat.Id, "Данная кнопка не поддерживается", request.CallbackQuery.Message.MessageId), cancellationToken);
                        break;
                }
                return default;
            }
        }
    }
}
