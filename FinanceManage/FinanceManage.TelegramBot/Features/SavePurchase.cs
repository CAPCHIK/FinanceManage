using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinanceManage.TelegramBot.Features
{
    public class SavePurchase
    {
        public record Command(
            int BuyerTelegramId,
            string Category,
            float Price,
            DateTimeOffset Date,
            long TelegramChannelId) : IRequest<bool>;

        public class CommandMapping : Profile
        {
            public CommandMapping()
            {
                CreateMap<Command, Purchase>();
            }
        }

        public class SavePurchaseHandler : IRequestHandler<Command, bool>
        {
            private readonly FinanceManageDbContext dbContext;
            private readonly IMapper mapper;
            private readonly ILogger<SavePurchaseHandler> logger;

            public SavePurchaseHandler(
                FinanceManageDbContext dbContext,
                IMapper mapper,
                ILogger<SavePurchaseHandler> logger)
            {
                this.dbContext = dbContext;
                this.mapper = mapper;
                this.logger = logger;
            }
            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var newModel = mapper.Map<Purchase>(request);
                dbContext.Purchases.Add(newModel);
                try
                {
                    var saved = await dbContext.SaveChangesAsync(cancellationToken);
                    return saved == 1;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "$Can't invoke {nameof(SavePurchase)}");
                    return false;
                }
            }
        }
    }
}