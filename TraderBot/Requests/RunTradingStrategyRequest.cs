using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TraderBot.Contracts;
using TraderBot.Models;
using TraderBot.Strategies;

namespace TraderBot.Requests
{
    public class RunTradingStrategyRequest : IRequest<RunTradingStrategyResult>
    {
        public string SymbolName { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public decimal Usd { get; set; }

        public ITradingStrategy Strategy { get; set; } = new BuyAndHoldStrategy();
    }

    public class RunTradingStrategyRequestHandler : IRequestHandler<RunTradingStrategyRequest, RunTradingStrategyResult>
    {
        private readonly TradingContext context;

        public RunTradingStrategyRequestHandler(TradingContext context)
        {
            this.context = context;
        }

        public async Task<RunTradingStrategyResult> Handle(RunTradingStrategyRequest request, CancellationToken cancellationToken)
        {
            var series = await context.TimeSeries
                    .FirstOrDefaultAsync(p => p.Symbol.Name == request.SymbolName && p.Interval == AlphaVantage.Net.Common.Intervals.Interval.Daily, cancellationToken);

            var data = context.StockDataPoints
                .Where(p => p.TimeSeriesId == series.Id);

            if (request.Start.HasValue)
            {
                data = data.Where(p => p.Time >= request.Start.Value);
            }

            if (request.End.HasValue)
            {
                data = data.Where(p => p.Time >= request.End.Value);
            }

            var datapoints = await data
                .OrderBy(p => p.Time)
                .ToListAsync(cancellationToken);
            ;

            var first = datapoints.First();
            var last = datapoints.Last();
            var actions = request.Strategy.Run(datapoints, request.Usd);
            var sell = actions.Last();

            var returnPercentage = (sell.Usd - request.Usd) / request.Usd * 100;

            var totalYears = (last.Time - first.Time).TotalDays / 356;
            var yearlyReturnPercentage = (Math.Pow((double)sell.Usd / (double)request.Usd, 1 / totalYears) - 1) * 100;

            return new RunTradingStrategyResult()
            {
                Usd = sell.Usd,
                TotalReturnPercentage = returnPercentage,
                YearlyReturnPercentage = (decimal)yearlyReturnPercentage,
                Actions = actions
            };
        }
    }
}
