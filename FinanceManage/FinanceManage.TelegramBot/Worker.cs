using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace FinanceManage.TelegramBot
{
    public class Worker : IHostedService
    {
        private readonly ITelegramBotClient telegramClient;
        private readonly ILogger<Worker> logger;

        public Worker(
            ITelegramBotClient telegramClient,
            ILogger<Worker> logger)
        {
            this.telegramClient = telegramClient;
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

        private void TelegramClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs args)
        {
            logger.LogInformation($"message id: {args.Message.MessageId}");
            logger.LogInformation($"message text: {args.Message?.Text}");
        }
    }
}
