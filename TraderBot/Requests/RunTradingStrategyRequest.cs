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
        public IReadOnlyList<string> SymbolNames { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public AlphaVantage.Net.Common.Intervals.Interval Interval { get; set; } = AlphaVantage.Net.Common.Intervals.Interval.Daily;

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
            var dataset = new Dictionary<string, IReadOnlyList<StockDataPoint>>(); 

            foreach (var symbol in request.SymbolNames)
            {
                dataset.Add(symbol, await ReadDataPoints(request, symbol, cancellationToken));
            }


            var datapoints = dataset.Values.First();
            var first = datapoints.First();
            var last = datapoints.Last();
            var actions = request.Strategy.Run(dataset, request.Usd);
            if (!actions.Any())
            {
                return new RunTradingStrategyResult()
                {
                    Usd = request.Usd,
                    Actions = actions
                };
            }


            var sell = actions.Last();

            foreach (var action in actions)
            {
                action.Usd = Math.Round(action.Usd, 2);
            }

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

        private async Task<List<StockDataPoint>> ReadDataPoints(RunTradingStrategyRequest request, string symbol, CancellationToken cancellationToken)
        {
            var series = await context.TimeSeries
                .Include(p => p.Symbol)
                .FirstOrDefaultAsync(p => p.Symbol.Name == symbol && p.Interval == request.Interval, cancellationToken);

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
            return datapoints;
        }
    }
}
