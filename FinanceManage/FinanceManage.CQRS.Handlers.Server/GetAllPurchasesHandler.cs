using AutoMapper;
using AutoMapper.QueryableExtensions;
using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FinanceManage.CQRS.Queries.GetAllPurchases;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class GetAllPurchasesHandlerProfile : Profile
    {
        public GetAllPurchasesHandlerProfile()
        {
            CreateMap<Purchase, Response>();
        }
    }
    public class GetAllPurchasesHandler : IRequestHandler<Command, List<Response>>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly IMapper mapper;

        public GetAllPurchasesHandler(FinanceManageDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }
        public Task<List<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            return dbContext.Purchases
                .Where(p => p.TelegramChannelId == request.ChatId)
                .ProjectTo<Response>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}
