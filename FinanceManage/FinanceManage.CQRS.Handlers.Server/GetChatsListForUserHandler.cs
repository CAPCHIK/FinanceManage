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
            var fromDb = await dbContext
                .Purchases
                .Where(p => p.BuyerTelegramId == request.UserId)
                .GroupBy(p => p.TelegramChatId)
                .Select(p => new GetChatsListForUser.Response(p.Key, "no chat name"))
                .ToListAsync(cancellationToken: cancellationToken);
            return fromDb;
        }
    }
}
