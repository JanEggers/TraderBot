namespace TraderBot.Requests;

public class RunTradingStrategyRequest : IRequest<RunTradingStrategyResult>
{
    public decimal Usd { get; set; }

    public ITradingStrategy Strategy { get; set; } = new BuyAndHoldStrategy();

    public Dataset Dataset { get; set; }
}

public class RunTradingStrategyRequestHandler : IRequestHandler<RunTradingStrategyRequest, RunTradingStrategyResult>
{
    public async Task<RunTradingStrategyResult> Handle(RunTradingStrategyRequest request, CancellationToken cancellationToken)
    {
        var datapoints = request.Dataset.Quotes.Values.First();
        var actions = request.Strategy.Run(request.Dataset.Quotes, request.Usd);
        if (!actions.Any())
        {
            return new RunTradingStrategyResult()
            {
                Strategy = request.Strategy,
                Usd = request.Usd,
                Actions = actions
            };
        }

        var sell = actions.Last();

        foreach (var action in actions)
        {
            action.Usd = Math.Round(action.Usd, 2);
        }

        var trends = actions.Trends(request.Dataset.Quotes).ToList();
        foreach (var item in trends)
        {
            item.Duration = item.Peak.Time - item.Start.Time;
            if (item.Bottom == item.Start)
            {
                item.Volatility = (item.Peak.AdjustedClosingPrice / item.Start.AdjustedClosingPrice - 1) * 100;
            }
            else
            {
                item.Volatility = (item.Bottom.AdjustedClosingPrice / item.Start.AdjustedClosingPrice - 1) * 100;
            }
        }

        var returnPercentage = (sell.Usd - request.Usd) / request.Usd * 100;

        var first = datapoints.First();
        var last = datapoints.Last();
        var totalYears = (last.Time - first.Time).TotalDays / 356;
        var yearlyReturnPercentage = (Math.Pow((double)sell.Usd / (double)request.Usd, 1 / totalYears) - 1) * 100;

        return new RunTradingStrategyResult()
        {
            Strategy = request.Strategy,
            Usd = sell.Usd,
            TotalReturnPercentage = returnPercentage,
            YearlyReturnPercentage = (decimal)yearlyReturnPercentage,
            Actions = actions,
            AverageDownTrendDuration = TimeSpan.FromTicks((long)trends.Select(t => t.Bottom.Time - t.Start.Time).Where(t => t.TotalDays > 0).Select(t => t.Ticks).Average()),
            TrendsUnsorted = trends,
            TrendsByVolatility = trends.OrderBy(p => p.Volatility).ToList(),
            TrendsByDuration = trends.OrderByDescending(p => p.Duration).ToList(),
            WorstTrend = trends.MinBy(p => p.Volatility),
            BestTrend = trends.MaxBy(p => p.Volatility)
        };
    }    
}
