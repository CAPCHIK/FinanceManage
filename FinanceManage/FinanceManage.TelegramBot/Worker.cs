using FinanceManage.TelegramBot.Features;
using FinanceManage.TelegramBot.InlineQueryModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinanceManage.TelegramBot
{
    public class Worker : IHostedService
    {
        private readonly ITelegramBotClient telegramClient;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<Worker> logger;

        private const string SpendingStatistic = "Статистика трат";
        private readonly IReadOnlyCollection<string> topLevelCommands = new List<string>
        {
            SpendingStatistic
        };

        private JsonSerializerOptions jsonInlineButtonOptions;

        public Worker(
            ITelegramBotClient telegramClient,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<Worker> logger)
        {
            this.telegramClient = telegramClient;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeOffsetConverter());
            jsonInlineButtonOptions = options;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await telegramClient.GetMeAsync(cancellationToken);
            logger.LogInformation($"Using Telegram bot {me.FirstName} id: {me.Id}");

            telegramClient.OnMessage += TelegramClient_OnMessage;
            telegramClient.OnCallbackQuery += TelegramClient_OnCallbackQuery; ;
            telegramClient.StartReceiving(cancellationToken: cancellationToken);
        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            telegramClient.StopReceiving();
            return Task.CompletedTask; // TODO save tasks and waiting
        }

        private async void TelegramClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs args)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await HandleMessage(mediator, args.Message);
        }

        private async void TelegramClient_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            try
            {
                await HandleCallbackQuery(mediator, e.CallbackQuery);
            }
            catch (MessageIsNotModifiedException ex)
            {
                logger.LogWarning(ex, "try to set same text for message");
            }
        }

        private async Task HandleCallbackQuery(IMediator mediator, CallbackQuery query)
        {
            InlineQueryBase data;
            try
            {
                data = JsonSerializer.Deserialize<InlineQueryBase>(query.Data, jsonInlineButtonOptions);
            }
            catch
            {
                await SendMainPanelMessage(query.Message.Chat.Id, "Данная кнопка не поддерживается", query.Message.MessageId);
                return;
            }
            switch (data.Command)
            {
                case CallbackQueryCommand.WeekSpendingStatistic:
                    logger.LogInformation(query.Data);
                    var weekSpendingStatisticData = JsonSerializer.Deserialize<WeekSpendingStatisticData>(query.Data, jsonInlineButtonOptions);
                    var (text, markup) = await PrepareWeekSpendingMessage(weekSpendingStatisticData.WeekStart, query.Message.Chat.Id, weekSpendingStatisticData.Category, mediator);
                    await telegramClient.EditMessageTextAsync(query.Message.Chat.Id, query.Message.MessageId, text, ParseMode.MarkdownV2, replyMarkup: markup);
                    break;
                default:
                    await SendMainPanelMessage(query.Message.Chat.Id, "Данная кнопка не поддерживается", query.Message.MessageId);
                    break;
            }
        }

        private async Task HandleMessage(IMediator mediator, Message message)
        {
            if (string.IsNullOrEmpty(message.Text))
            {
                await SendMainPanelMessage(message.Chat, $"Некорректное сообщение", replyToMessageId: message.MessageId);
                return;
            }
            if (topLevelCommands.Contains(message.Text))
            {
                await HandleTopLevelCommand(message, mediator);
                return;
            }

            var lines = message.Text.Split('\n');
            SavePurchase.Command command = null;
            if (lines.Length == 2)
            {
                command = await HandleCategoryAndPrice(lines, message);
            }
            else if (lines.Length == 3)
            {
                command = await HandleFullInfo(lines, message);
            }
            else
            {
                await SendMainPanelMessage(message.Chat, $"Некорректный формат сообщения. Необходимо вписать или три (категория, дата, сумма), или две (категория, сумма) строки.", replyToMessageId: message.MessageId);
            }

            if (command == null)
            {
                logger.LogInformation($"Incorrect input message");
                return;
            }
            var saveResult = await mediator.Send(command);
            if (saveResult)
            {
                await SendMainPanelMessage(message.Chat, BuildSavedPurchaseMessage(command), parseMode: ParseMode.MarkdownV2, replyToMessageId: message.MessageId);
            }
            else
            {
                await SendMainPanelMessage(message.Chat, $"Ошибка при сохранении", replyToMessageId: message.MessageId);
            }

        }

        private async Task HandleTopLevelCommand(Message message, IMediator mediator)
        {
            switch (message.Text)
            {
                case SpendingStatistic:
                    await HandleSpendingStatistic(message, mediator);
                    break;
                default:
                    await SendMainPanelMessage(message.Chat.Id, $"Некорректная команда", message.MessageId);
                    break;
            }
        }

        private async Task HandleSpendingStatistic(Message message, IMediator mediator)
        {
            var (text, markup) = await PrepareWeekSpendingMessage(message.Date.Date.AddDays(-6), message.Chat.Id, WeekSpending.CategoryMode.Compact, mediator);
            await SendMainPanelMessage(message.Chat.Id, text, message.MessageId, ParseMode.MarkdownV2, markup);
        }

        private async Task<(string text, InlineKeyboardMarkup markup)> PrepareWeekSpendingMessage(DateTimeOffset start, long chatId, WeekSpending.CategoryMode categoryMode, IMediator mediator)
        {
            var result = await mediator.Send(new WeekSpending.Command(start, chatId, categoryMode));
            string resultText = BuildWeekSpendingMessage(result);

            var callbackDataPreviousWeek = new WeekSpendingStatisticData(result.From.AddDays(-7), categoryMode);
            var callbackDataPreviousWeekJson = JsonSerializer.Serialize(callbackDataPreviousWeek, jsonInlineButtonOptions);

            var callbackDataNextWeek = new WeekSpendingStatisticData(result.To, categoryMode);
            var callbackDataNextWeekJson = JsonSerializer.Serialize(callbackDataNextWeek, jsonInlineButtonOptions);

            InlineKeyboardButton categoryModeButton;
            switch (categoryMode)
            {
                case WeekSpending.CategoryMode.Compact:
                    var toComplete = new WeekSpendingStatisticData(start, WeekSpending.CategoryMode.Complete);
                    var toCompleteJson = JsonSerializer.Serialize(toComplete, jsonInlineButtonOptions);
                    categoryModeButton = InlineKeyboardButton.WithCallbackData(Emoji.HearNoEvilMonkey, toCompleteJson);
                    break;
                case WeekSpending.CategoryMode.Complete:
                    var toCompact = new WeekSpendingStatisticData(start, WeekSpending.CategoryMode.Compact);
                    var toCompactJson = JsonSerializer.Serialize(toCompact, jsonInlineButtonOptions);
                    categoryModeButton = InlineKeyboardButton.WithCallbackData(Emoji.SeeNoEvilMonkey, toCompactJson);
                    break;
                default:
                    throw new ArgumentException("incorrect category", nameof(categoryMode));
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

            return (resultText, inlineMarkup);
        }

        private static string BuildWeekSpendingMessage(WeekSpending.Result result)
        {
            var builder = new StringBuilder();
            builder.Append(result.From.CompactMarkdownV2Date());
            builder.Append(" \\- ");
            builder.Append(result.To.AddMinutes(-1).CompactMarkdownV2Date());
            builder.Append(": `");
            builder.Append(result.Sum.ToMoneyString());
            builder.AppendLine("₽`");
            foreach (var categoryRecord in result.ByCategory.OrderByDescending(cr => cr.Value))
            {
                builder.Append(Escape(categoryRecord.Key));
                builder.Append(": `");
                builder.Append(categoryRecord.Value.ToMoneyString());
                builder.AppendLine("₽`");
            }


            return builder.ToString();
        }


        private static string BuildSavedPurchaseMessage(SavePurchase.Command command)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Сохранено");

            builder.Append(@"Категория: `");
            builder.Append(Escape(command.Category));
            builder.AppendLine("`");

            builder.Append(@"Сумма: `");
            builder.Append(command.Price.ToMoneyString());
            builder.AppendLine("₽`");

            builder.Append(@"Дата: `");
            builder.Append(command.Date.ToString("yyyy.MM.dd"));
            builder.AppendLine("`");

            return builder.ToString();
        }

        private async Task<SavePurchase.Command> HandleFullInfo(string[] lines, Message message)
        {
            var (categorySuccess, category) = await ParseCategoryFromLine(lines[0], message);
            if (!categorySuccess)
            {
                return null;
            }
            if (!ParseDateFromLine(lines[1], message.Date.Year, out var date))
            {
                await SendMainPanelMessage(message.Chat, $"Некорректный формат даты. Необходимо указать день.месяц", replyToMessageId: message.MessageId);
                return null;
            }
            var price = await GetPriceFromLine(lines[2], message);
            if (price == null)
            {
                return null;
            }
            return new SavePurchase.Command(
                message.From.Id,
                category,
                price.Value,
                date,
                message.Chat.Id);
        }

        private async Task<SavePurchase.Command> HandleCategoryAndPrice(string[] lines, Message message)
        {
            var (categorySuccess, category) = await ParseCategoryFromLine(lines[0], message);
            if (!categorySuccess)
            {
                return null;
            }
            var price = await GetPriceFromLine(lines[1], message);
            if (price == null)
            {
                return null;
            }
            return new SavePurchase.Command(
                message.From.Id,
                category,
                price.Value,
                message.Date,
                message.Chat.Id);
        }

        private async Task<float?> GetPriceFromLine(string line, Message message)
        {
            if (!float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out var price) || float.IsNaN(price))
            {
                await SendMainPanelMessage(message.Chat, $"Некорректный формат суммы. Необходимо использовать число.", replyToMessageId: message.MessageId);
                return null;
            }
            if (price <= 0)
            {
                await SendMainPanelMessage(message.Chat, $"Сумма может быть только положительной", replyToMessageId: message.MessageId);
                return null;
            }
            if (float.IsInfinity(price))
            {
                await SendMainPanelMessage(message.Chat, $"К сожалению, ваше число не входит в рамки доступных для сохранения", replyToMessageId: message.MessageId);
                return null;
            }
            price = (float)Math.Round(price, 2); // TODO use decimal #1
            return price;
        }

        private async Task SendMainPanelMessage(ChatId chatId, string text, int replyToMessageId, ParseMode parseMode = ParseMode.Default, IReplyMarkup replyMarkup = default)
        {
            replyMarkup ??= new ReplyKeyboardMarkup(topLevelCommands.Select(c => new KeyboardButton[] { new KeyboardButton(c) }), resizeKeyboard: true);
            await telegramClient.SendTextMessageAsync(chatId, text, parseMode: parseMode, replyToMessageId: replyToMessageId,
                replyMarkup: replyMarkup);
        }

        private async Task<(bool success, string category)> ParseCategoryFromLine(string line, Message message)
        {
            if (string.IsNullOrEmpty(line))
            {
                await SendMainPanelMessage(message.Chat, "Необходимо ввести категорию", replyToMessageId: message.MessageId);
                return (false, default);
            }
            if (line.Length > 100)
            {
                await SendMainPanelMessage(message.Chat, "Максимальная длина категории - 100 символов", replyToMessageId: message.MessageId);
                return (false, default);
            }
            return (true, line);
        }

        private bool ParseDateFromLine(string line, int year, out DateTimeOffset date)
        {
            try
            {
                var tokens = line.Split('.').Select(int.Parse).ToArray();
                date = new DateTimeOffset(year, tokens[1], tokens[0], 0, 0, 0, TimeSpan.Zero);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "can't parse date");
                date = default;
                return false;
            }
        }

        private static string Escape(string text)
        {
            return Regex.Replace(text, @"_|\*|\[|\]|\(|\)|~|`|>|#|\+|-|=|\\|{|}|\.|!", m => $"\\{m.Value}");
        }
    }
}
