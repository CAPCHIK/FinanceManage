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
    public class Worker : BackgroundService
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            telegramClient.OnMessage += TelegramClient_OnMessage;
            telegramClient.StartReceiving(cancellationToken: stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                var me = await telegramClient.GetMeAsync(stoppingToken);
                logger.LogInformation(me.FirstName);
                await Task.Delay(1000, stoppingToken);
            }
            telegramClient.StopReceiving();
        }

        private void TelegramClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs args)
        {
            logger.LogInformation($"message: {args.Message.Text}");
        }
    }
}
