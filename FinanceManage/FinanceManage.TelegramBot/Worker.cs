using FinanceManage.TelegramBot.Features;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
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

        private const string WeekSpending = "Траты за неделю";
        private readonly IReadOnlyCollection<string> topLevelCommands = new List<string>
        {
            WeekSpending
        };

        public Worker(
            ITelegramBotClient telegramClient,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<Worker> logger)
        {
            this.telegramClient = telegramClient;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var me = await telegramClient.GetMeAsync(cancellationToken);
            logger.LogInformation($"Using Telegram bot {me.FirstName} id: {me.Id}");

            telegramClient.OnMessage += TelegramClient_OnMessage;
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
                case WeekSpending:
                    var result = await mediator.Send(new WeekSpending.Command(new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero).AddDays(-7), message.Chat.Id));
                    if (result.Sum == 0)
                    {
                        await SendMainPanelMessage(message.Chat.Id, $"Нет трат за последнюю неделю", message.MessageId);
                    }
                    else
                    {
                        await SendMainPanelMessage(message.Chat.Id, BuildWeekSpendingMessage(result), message.MessageId, ParseMode.MarkdownV2);
                    }
                    break;
                default:
                    await SendMainPanelMessage(message.Chat.Id, $"Некорректная команда", message.MessageId);
                    break;
            }
        }

        private static string BuildWeekSpendingMessage(WeekSpending.Result result)
        {
            var builder = new StringBuilder();
            builder.Append("За последнюю неделю потрачено: `");
            builder.Append(result.Sum.ToMoneyString());
            builder.AppendLine("₽`");
            foreach (var categoryRecord in result.ByCategory.OrderByDescending(cr => cr.Value))
            {
                builder.Append(categoryRecord.Key);
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
            builder.Append(command.Category);
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
            var category = lines[0];
            if (!ParseDateFromLine(lines[1], out var date))
            {
                await SendMainPanelMessage(message.Chat, $"Некорректный формат даты. Необходимо указать день.месяц", replyToMessageId: message.MessageId);
                return null;
            }
            var price = await GetProceFromLine(lines[1], message);
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
            var category = lines[0];
            var price = await GetProceFromLine(lines[1], message);
            if (price == null)
            {
                return null;
            }
            return new SavePurchase.Command(
                message.From.Id,
                category,
                price.Value,
                DateTimeOffset.UtcNow,
                message.Chat.Id);
        }

        private async Task<float?> GetProceFromLine(string line, Message message)
        {
            if (!float.TryParse(line, out var price))
            {
                await SendMainPanelMessage(message.Chat, $"Некорректный формат суммы на второй строке. Необходимо число.", replyToMessageId: message.MessageId);
                return null;
            }
            if (price <= 0)
            {
                await SendMainPanelMessage(message.Chat, $"Сумма может быть только положительной", replyToMessageId: message.MessageId);
                return null;
            }
            return price;
        }

        private async Task SendMainPanelMessage(ChatId chatId, string text, int replyToMessageId, ParseMode parseMode = ParseMode.Default)
        {
            await telegramClient.SendTextMessageAsync(chatId, text, parseMode: parseMode, replyToMessageId: replyToMessageId,
                replyMarkup: new ReplyKeyboardMarkup(topLevelCommands.Select(c => new KeyboardButton[] { new KeyboardButton(c) }), resizeKeyboard: true));
        }

        private static bool ParseDateFromLine(string line, out DateTimeOffset date)
        {
            try
            {
                var tokens = line.Split('.').Select(int.Parse).ToArray();
                date = new DateTimeOffset(DateTimeOffset.UtcNow.Year, tokens[1], tokens[0], 0, 0, 0, TimeSpan.Zero);
                return true;
            }
            catch
            {
                date = default;
                return false;
            }
        }
    }
}
