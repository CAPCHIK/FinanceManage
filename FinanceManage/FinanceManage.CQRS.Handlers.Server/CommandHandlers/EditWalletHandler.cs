using AutoMapper;
using FinanceManage.Database;
using FinanceManage.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static FinanceManage.CQRS.Commands.EditWallet;

namespace FinanceManage.CQRS.Handlers.Server.CommandHandlers
{
    public class EditWalletMapping : Profile
    {
        public EditWalletMapping()
        {
            CreateMap<Command, Wallet>()
                .ForMember(w => w.TelegramChatId, map => map.Ignore());
        }
    }
    class EditWalletHandler : IRequestHandler<Command, Result>
    {
        private readonly FinanceManageDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<CreateWalletHandler> logger;

        public EditWalletHandler(
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
            var targetWallet = await dbContext.Wallets.SingleOrDefaultAsync(w => w.Id == request.WalletId);
            if (targetWallet == null)
            {
                return new Result(ErrorReason.NotFound);
            }
            var targetModel = mapper.Map(request, targetWallet);

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
