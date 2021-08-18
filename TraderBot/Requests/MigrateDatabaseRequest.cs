using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TraderBot.Models;

namespace TraderBot.Requests
{
    public class MigrateDatabaseRequest : IRequest
    {
    }

    public class MigrateDatabaseRequestHandler : IRequestHandler<MigrateDatabaseRequest>
    {
        private readonly TradingContext tradingContext;

        public MigrateDatabaseRequestHandler(TradingContext tradingContext)
        {
            this.tradingContext = tradingContext;
        }

        public async Task<Unit> Handle(MigrateDatabaseRequest request, CancellationToken cancellationToken)
        {
            await tradingContext.Database.MigrateAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
