using AutoMapper;
using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Commands.CreateWallet;

namespace FinanceManage.CQRS.Handlers.Server.CommandHandlers
{
    public class CreateWalletMapping : Profile
    {
        public CreateWalletMapping()
        {
            CreateMap<Command, Wallet>();
        }
    }
    class CreateWalletHandler : IRequestHandler<Command, Result>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<CreateWalletHandler> logger;

        public CreateWalletHandler(
            FinanceManageDbContext dbContext,
            IMapper mapper,
            ILogger<CreateWalletHandler> logger
            )
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
        }
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var walletModel = mapper.Map<Wallet>(request);
            dbContext.Wallets.Add(walletModel);
            try
            {
                var saved = await dbContext.SaveChangesAsync(cancellationToken);
                return new Result(null);
            }
            catch (Exception ex) 
            when (ex.InnerException is PostgresException pe &&
                    pe.SqlState == PostgresErrorCodes.UniqueViolation &&
                    pe.ConstraintName.Contains(nameof(Wallet.Title)))
            {
                return new Result(ErrorReason.TitleExists);
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, $"Can't invoke {nameof(Commands.CreateWallet)}");
                return new Result(ErrorReason.UnexpectedError);
            }
        }
    }
}
