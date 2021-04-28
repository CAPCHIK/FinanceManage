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
using Telegram.Bot.Types.ReplyMarkups;

namespace FinanceManage.TelegramBot.Features.Telegram
{
    public class PrepareWeekSpendingMessage
    {
        public record Response(string Text, InlineKeyboardMarkup Markup);
        public record Command(DateTimeOffset StartDay, long ChatId, WeekSpending.CategoryMode CategoryMode) : IRequest<Response>;

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly IMediator mediator;
            private readonly ILogger<Handler> logger;

            public Handler(IMediator mediator, ILogger<Handler> logger)
            {
                this.mediator = mediator;
                this.logger = logger;
            }
            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = await mediator.Send(new WeekSpending.Command(
                    request.StartDay,
                    request.ChatId,
                    request.CategoryMode), cancellationToken);
                string resultText = BuildWeekSpendingMessage(result);

                logger.LogDebug($"week message: {resultText}");

                var callbackDataPreviousWeek = new WeekSpendingStatisticData(result.From.AddDays(-7), request.CategoryMode);
                var callbackDataPreviousWeekJson = JsonSerializer.Serialize(callbackDataPreviousWeek, JsonOptions.InlineJeyboardOptions.Value);

                var callbackDataNextWeek = new WeekSpendingStatisticData(result.To, request.CategoryMode);
                var callbackDataNextWeekJson = JsonSerializer.Serialize(callbackDataNextWeek, JsonOptions.InlineJeyboardOptions.Value);

                InlineKeyboardButton categoryModeButton;
                switch (request.CategoryMode)
                {
                    case WeekSpending.CategoryMode.Compact:
                        var toComplete = new WeekSpendingStatisticData(request.StartDay, WeekSpending.CategoryMode.Complete);
                        var toCompleteJson = JsonSerializer.Serialize(toComplete, JsonOptions.InlineJeyboardOptions.Value);
                        categoryModeButton = InlineKeyboardButton.WithCallbackData(Emoji.HearNoEvilMonkey, toCompleteJson);
                        break;
                    case WeekSpending.CategoryMode.Complete:
                        var toCompact = new WeekSpendingStatisticData(request.StartDay, WeekSpending.CategoryMode.Compact);
                        var toCompactJson = JsonSerializer.Serialize(toCompact, JsonOptions.InlineJeyboardOptions.Value);
                        categoryModeButton = InlineKeyboardButton.WithCallbackData(Emoji.SeeNoEvilMonkey, toCompactJson);
                        break;
                    default:
                        throw new ArgumentException("incorrect category", nameof(request));
                }

                var buttons = new InlineKeyboardButton[][] {

                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{result.From.AddDays(-7):dd.MM}-{result.From.AddMinutes(-1):dd.MM}", callbackDataPreviousWeekJson),

                    InlineKeyboardButton.WithCallbackData(
                        $"{result.To:dd.MM}-{result.To.AddDays(7).AddMinutes(-1):dd.MM}",callbackDataNextWeekJson )
                },
                new InlineKeyboardButton[]
                {
                       categoryModeButton
                },
            };
                var inlineMarkup = new InlineKeyboardMarkup(buttons);

                return new(resultText, inlineMarkup);
            }


            private static string BuildWeekSpendingMessage(WeekSpending.Result result)
            {
                var builder = new StringBuilder();
                builder.Append(result.From.CompactMarkdownV2Date());
                builder.Append(" - ".EscapeAsMarkdownV2());
                builder.Append(result.To.AddMinutes(-1).CompactMarkdownV2Date());
                builder.Append(": *__");
                builder.Append(result.Sum.ToMoneyString().EscapeAsMarkdownV2());
                builder.AppendLine("__₽*");
                foreach (var categoryRecord in result.ByCategory.OrderByDescending(cr => cr.Value))
                {
                    builder.Append(categoryRecord.Key.EscapeAsMarkdownV2());
                    builder.Append(": `");
                    builder.Append(categoryRecord.Value.ToMoneyString());
                    builder.AppendLine("₽`");
                }


                return builder.ToString();
            }
        }
    }
}
