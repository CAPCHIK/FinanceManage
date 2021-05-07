using AutoMapper;
using AutoMapper.QueryableExtensions;
using FinanceManage.CQRS.Queries;
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
using static FinanceManage.CQRS.Queries.GetPurchases;

namespace FinanceManage.CQRS.Handlers.Server
{
    public class GetPurchasesHandlerProfile : Profile
    {
        public GetPurchasesHandlerProfile()
        {
            CreateMap<Purchase, Response>();
        }
    }
    public class GetPurchasesHandler : IRequestHandler<Command, ListWrapper<Response>>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly IMapper mapper;

        public GetPurchasesHandler(FinanceManageDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }
        public async Task<ListWrapper<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var selection = dbContext.Purchases
                .Where(p => p.TelegramChatId == request.ChatId);
            var list = await selection
                .OrderBy(p => p.Date)
                .Skip(request.PageNum * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<Response>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken: cancellationToken);
            var total = await selection.CountAsync(cancellationToken: cancellationToken);
            return new ListWrapper<Response>(request.PageNum, request.PageSize, total, list);
        }
    }
}
