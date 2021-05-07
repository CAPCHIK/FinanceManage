using FinanceManage.CQRS.Notifications;
using FinanceManage.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class UpdateTelegramChatTitleHandler : INotificationHandler<UpdateTelegramChatTitle>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly ITelegramBotClient telegramBotClient;
        private readonly ILogger<UpdateTelegramChatTitleHandler> logger;

        private static string cachedBotName;

        public UpdateTelegramChatTitleHandler(
            FinanceManageDbContext dbContext,
            ITelegramBotClient telegramBotClient,
            ILogger<UpdateTelegramChatTitleHandler> logger)
        {
            this.dbContext = dbContext;
            this.telegramBotClient = telegramBotClient;
            this.logger = logger;
        }
        public async Task Handle(UpdateTelegramChatTitle notification, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(cachedBotName))
            {
                cachedBotName = (await telegramBotClient.GetMeAsync(cancellationToken)).FirstName;
            }
            var targetCacheRow = await dbContext.TelegramChatInfoCache
                .SingleOrDefaultAsync(r => r.Id == notification.ChatId, cancellationToken: cancellationToken);

            var latestChatName = notification.Title ?? cachedBotName;
            if (targetCacheRow == null)
            {
                dbContext.TelegramChatInfoCache.Add(new Models.TelegramChatInfoCache
                {
                    Id = notification.ChatId,
                    Title = latestChatName,
                    CachedDate = DateTimeOffset.UtcNow
                });
            }
            else
            {
                if (targetCacheRow.Title != latestChatName)
                {
                    targetCacheRow.Title = latestChatName;
                    targetCacheRow.CachedDate = DateTimeOffset.UtcNow;
                }
            }
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Can't update cache");
            }
        }
    }
}
