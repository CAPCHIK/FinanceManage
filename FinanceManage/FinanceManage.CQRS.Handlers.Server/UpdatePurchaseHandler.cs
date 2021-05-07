using AutoMapper;
using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Queries.UpdatePurchase;

namespace FinanceManage.CQRS.Handlers.Server
{
    class UpdatePurchaseMapper : Profile
    {
        public UpdatePurchaseMapper()
        {
            CreateMap<Command, Purchase>()
                .ForMember(p => p.Id, map => map.MapFrom(c => c.PurchaseId));
        }
    }
    class UpdatePurchaseHandler : IRequestHandler<Command, bool>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<UpdatePurchaseHandler> logger;

        public UpdatePurchaseHandler(
            FinanceManageDbContext dbContext, 
            IMapper mapper,
            ILogger<UpdatePurchaseHandler> logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
        }
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var targetPurchase = await dbContext.Purchases
                    .SingleOrDefaultAsync(p => p.Id == request.PurchaseId, cancellationToken: cancellationToken);
                if (targetPurchase == null)
                {
                    return false;
                }
                mapper.Map(request, targetPurchase);
                var saved = await dbContext.SaveChangesAsync(cancellationToken);
                return saved == 1;
            } catch (Exception ex)
            {
                logger.LogWarning(ex, "can't update purchase");
                return false;
            }
        }
    }
}
