using FinanceManage.CQRS.Queries;
using FinanceManage.TelegramBot.Features;
using FinanceManage.TelegramBot.Features.Telegram;
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
            try
            {
                await HandleMessage(mediator, args.Message);
            }
            catch (ApiRequestException apiEx)
            {
                logger.LogError(apiEx, "Error while handling message");
            }
        }

        private async void TelegramClient_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            try
            {
                await mediator.Send(new HandleCallbackQuery.Command(e.CallbackQuery));
            }
            catch (MessageIsNotModifiedException ex)
            {
                logger.LogWarning(ex, "try to set same text for message");
            }
        }

        private async Task HandleMessage(IMediator mediator, Message message)
        {
            if (string.IsNullOrEmpty(message.Text))
            {
                await mediator.Send(new SendMessage.Command(message.Chat.Id, $"Некорректное сообщение", message.MessageId));
                return;
            }
            var isTopCommand = await mediator.Send(new HandleTopLevelCommandMessage.Command(message));
            if (isTopCommand)
            {
                return;
            }
            var lines = message.Text.Split('\n');
            bool success;
            SavePurchase.Command command = null;
            string errorMessage;
            if (lines.Length == 2)
            {
                success = TryHandleCategoryAndPrice(lines, message, out command, out errorMessage);
            }
            else if (lines.Length == 3)
            {
                success = TryHandleFullInfo(lines, message, out command, out errorMessage);
            }
            else
            {
                success = false;
                errorMessage = $"Некорректный формат сообщения. Необходимо вписать или три (категория, дата, сумма), или две (категория, сумма) строки.";
            }

            if (!success)
            {
                await mediator.Send(new SendMessage.Command(message.Chat.Id, errorMessage.EscapeAsMarkdownV2(), message.MessageId, ParseMode.MarkdownV2));
                return;
            }
            var saveResult = await mediator.Send(command);
            if (saveResult)
            {
                var today = await mediator.Send(new GetAverageSpendingPerDay.Command(message.Chat.Id, DateTimeOffset.UtcNow, TimeSpan.FromDays(1)));
                var averageMonth = await mediator.Send(new GetAverageSpendingPerDay.Command(message.Chat.Id, DateTimeOffset.UtcNow.AddDays(-30), TimeSpan.FromDays(30)));
                var reportText = BuildSavedPurchaseMessage(command, today, averageMonth);
                logger.LogDebug($"Send saved purchase report: >>{reportText}<<");
                await mediator.Send(new SendMessage.Command(message.Chat.Id, reportText, message.MessageId, ParseMode.MarkdownV2));
            }
            else
            {
                await mediator.Send(new SendMessage.Command(message.Chat.Id, $"Ошибка при сохранении", message.MessageId));
            }

        }


        private static string BuildSavedPurchaseMessage(SavePurchase.Command command, float today, float averageMonth)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{Emoji.MailWithDownArrow} Сохранено");
            builder.AppendLine();

            string categoryRow = command.Category;
            var words = categoryRow.Split(' ');

            builder.Append(@$"{Emoji.Shopping} *{words[0].EscapeAsMarkdownV2()}*");
            if (words.Length > 1)
            {
                builder.AppendLine($" _{string.Join(' ', words.Skip(1)).EscapeAsMarkdownV2()}_");
            } else
            {
                builder.AppendLine();
            }

            builder.AppendLine(@$"{Emoji.Money} {command.Price.ToMoneyString().EscapeAsMarkdownV2()}₽");
            builder.AppendLine(@$"{Emoji.Calendar} {command.Date.ToString("yyyy.MM.dd").EscapeAsMarkdownV2()}");
            builder.AppendLine();
            builder.AppendLine(@$"{Emoji.BarChart} сегодня / среднее / %");
            var averajeEmoji = today > averageMonth ? Emoji.ChartWithDownwardsTrend : Emoji.ChartWithUpwardsTrend;
            builder.AppendLine(@$"{averajeEmoji} {today.ToMoneyString().EscapeAsMarkdownV2()}₽ / {averageMonth.ToMoneyString().EscapeAsMarkdownV2()}₽ / {(int)(today / averageMonth * 100)}%");

            return builder.ToString();
        }

        private static bool TryHandleFullInfo(string[] lines, Message message, out SavePurchase.Command command, out string errorMessage)
        {
            if (!TryParseCategoryFromLine(lines[0], out var category, out errorMessage))
            {
                command = default;
                return false;
            }
            if (!TryParseDateFromLine(lines[1], message.Date.Year, out var date))
            {
                errorMessage = "Некорректный формат даты. Необходимо указать день.месяц";
                command = default;
                return false;
            }
            if (!TryGetPriceFromLine(lines[2], out var price, out errorMessage))
            {
                command = default;
                return false;
            }
            command = new SavePurchase.Command(
                message.From.Id,
                category,
                price,
                date,
                message.Chat.Id);
            return true;
        }

        private static bool TryHandleCategoryAndPrice(string[] lines, Message message, out SavePurchase.Command command, out string errorMessage)
        {
            if (!TryParseCategoryFromLine(lines[0], out var category, out errorMessage))
            {
                command = default;
                return false;
            }
            if (!TryGetPriceFromLine(lines[1], out var price, out errorMessage))
            {
                command = default;
                return false;
            }
            command = new SavePurchase.Command(
                message.From.Id,
                category,
                price,
                message.Date,
                message.Chat.Id);
            return true;
        }

        private static bool TryGetPriceFromLine(string line, out float price, out string errorMesage)
        {
            if (!float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out price) || float.IsNaN(price))
            {
                errorMesage = "Некорректный формат суммы. Необходимо использовать число.";
                return false;
            }
            if (price <= 0)
            {
                errorMesage = "Сумма может быть только положительной";
                return false;
            }
            if (float.IsInfinity(price))
            {
                errorMesage = "К сожалению, ваше число не входит в рамки доступных для сохранения";
                return false;
            }
            errorMesage = default;
            price = (float)Math.Round(price, 2); // TODO use decimal #1
            return true;
        }

        private static bool TryParseCategoryFromLine(string line, out string category, out string errorMessage)
        {
            if (string.IsNullOrEmpty(line))
            {
                category = default;
                errorMessage = "Необходимо ввести категорию";
                return false;
            }
            if (line.Length > 100)
            {
                category = default;
                errorMessage = "Максимальная длина категории - 100 символов";
                return false;
            }
            category = line;
            errorMessage = default;
            return true;
        }

        private static bool TryParseDateFromLine(string line, int year, out DateTimeOffset date)
        {
            try
            {
                var tokens = line.Split('.').Select(int.Parse).ToArray();
                date = new DateTimeOffset(year, tokens[1], tokens[0], 0, 0, 0, TimeSpan.Zero);
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
