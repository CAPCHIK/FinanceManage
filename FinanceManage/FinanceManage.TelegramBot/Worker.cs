using FinanceManage.TelegramBot.Features;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

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
                await telegramClient.SendTextMessageAsync(message.Chat, $"Некорректное сообщение", replyToMessageId: message.MessageId);
                return;
            }
            var lines = message.Text.Split('\n');
            if (lines.Length != 3)
            {
                await telegramClient.SendTextMessageAsync(message.Chat, $"Некорректный формат сообщения", replyToMessageId: message.MessageId);
                return;
            }
            var category = lines[0];
            if (!ParseDateFromLine(lines[1], out var date))
            {
                await telegramClient.SendTextMessageAsync(message.Chat, $"Некорректный формат даты. Необходимо указать день.месяц", replyToMessageId: message.MessageId);
                return;
            }
            if (!float.TryParse(lines[2], out var price))
            {
                await telegramClient.SendTextMessageAsync(message.Chat, $"Некорректный формат количества денег. Необходимо число", replyToMessageId: message.MessageId);
                return;
            }
            var saveResult = await mediator.Send(new SavePurchase.Command(
                message.From.Id,
                category,
                price,
                date,
                message.Chat.Id));
            if (saveResult)
            {
                await telegramClient.SendTextMessageAsync(message.Chat, $"Записано", replyToMessageId: message.MessageId);
            }
            else
            {
                await telegramClient.SendTextMessageAsync(message.Chat, $"Ошибка при сохранении", replyToMessageId: message.MessageId);
            }
        }

        private bool ParseDateFromLine(string line, out DateTimeOffset date)
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
