using AutoMapper;
using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.SavePurchase;


namespace FinanceManage.CQRS.Handlers.Server
{
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
                logger.LogError(ex, $"Can't invoke {nameof(Queries.SavePurchase)}");
                return false;
            }
        }
    }
}
