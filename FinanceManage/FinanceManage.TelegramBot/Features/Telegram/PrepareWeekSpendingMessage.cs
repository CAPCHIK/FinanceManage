using FinanceManage.CQRS.Queries;
using FinanceManage.Models.ServerSide.Options;
using FinanceManage.TelegramBot.InlineQueryModels;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinanceManage.TelegramBot.Features.Telegram
{
    public class PrepareWeekSpendingMessage
    {
        public record Response(string Text, InlineKeyboardMarkup Markup);
        public record Command(DateTimeOffset StartDay, long ChatId, AverageSpending.CategoryMode CategoryMode) : IRequest<Response>;

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly IOptions<TelegramBotOptions> options;
            private readonly IMediator mediator;
            private readonly ILogger<Handler> logger;

            public Handler(
                IOptions<TelegramBotOptions> options,
                IMediator mediator,
                ILogger<Handler> logger)
            {
                this.options = options;
                this.mediator = mediator;
                this.logger = logger;
            }
            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = await mediator.Send(new AverageSpending.Command(
                    request.StartDay, 7,
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
                    case AverageSpending.CategoryMode.Compact:
                        var toComplete = new WeekSpendingStatisticData(request.StartDay, AverageSpending.CategoryMode.Complete);
                        var toCompleteJson = JsonSerializer.Serialize(toComplete, JsonOptions.InlineJeyboardOptions.Value);
                        categoryModeButton = InlineKeyboardButton.WithCallbackData(Emoji.HearNoEvilMonkey, toCompleteJson);
                        break;
                    case AverageSpending.CategoryMode.Complete:
                        var toCompact = new WeekSpendingStatisticData(request.StartDay, AverageSpending.CategoryMode.Compact);
                        var toCompactJson = JsonSerializer.Serialize(toCompact, JsonOptions.InlineJeyboardOptions.Value);
                        categoryModeButton = InlineKeyboardButton.WithCallbackData(Emoji.SeeNoEvilMonkey, toCompactJson);
                        break;
                    default:
                        throw new ArgumentException("incorrect category", nameof(request));
                }
                var siteUrl = new UriBuilder(options.Value.SiteBaseAddres)
                {
                    Path = $"weekStatistic/{HttpUtility.UrlEncode(request.ChatId.ToString())}",
                    Query = $"weekStart={HttpUtility.UrlEncode(request.StartDay.ToString("yyyy-MM-dd"))}&category={HttpUtility.UrlEncode(request.CategoryMode.ToString())}"
                };

                var siteGraphButton = InlineKeyboardButton.WithUrl($"Посмотреть на сайте", siteUrl.ToString());

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
                    new InlineKeyboardButton[]
                    {
                           siteGraphButton
                    },
                };
                var inlineMarkup = new InlineKeyboardMarkup(buttons);

                return new(resultText, inlineMarkup);
            }


            private static string BuildWeekSpendingMessage(AverageSpending.Result result)
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
