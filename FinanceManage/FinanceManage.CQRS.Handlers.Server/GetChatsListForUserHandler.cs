using FinanceManage.CQRS.Queries;
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
using Telegram.Bot.Types.Enums;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class GetChatsListForUserHandler : IRequestHandler<GetChatsListForUser.Command, List<GetChatsListForUser.Response>>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly ITelegramBotClient telegramBotClient;
        private readonly ILogger<GetChatsListForUserHandler> logger;

        public GetChatsListForUserHandler(
            FinanceManageDbContext dbContext, 
            ITelegramBotClient telegramBotClient,
            ILogger<GetChatsListForUserHandler> logger)
        {
            this.dbContext = dbContext;
            this.telegramBotClient = telegramBotClient;
            this.logger = logger;
        }
        public async Task<List<GetChatsListForUser.Response>> Handle(GetChatsListForUser.Command request, CancellationToken cancellationToken)
        {
            var ids = await dbContext
                .Purchases
                .Where(p => p.BuyerTelegramId == request.UserId)
                .GroupBy(p => p.TelegramChatId)
                .Select(p => p.Key)
                .ToListAsync(cancellationToken: cancellationToken);

            var names = (await dbContext.TelegramChatInfoCache
                .Where(c => ids.Contains(c.Id))
                .ToListAsync(cancellationToken: cancellationToken)).ToDictionary(c => c.Id, c => c.Title);

            var result = ids.Select(id => new GetChatsListForUser.Response(
                id,
                names.TryGetValue(id, out var title) ? title : "no title"))
                .ToList();

            return result;
        }
    }
}
