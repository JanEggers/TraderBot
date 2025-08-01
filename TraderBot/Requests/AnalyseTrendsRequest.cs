namespace TraderBot.Requests;

public class AnalyseTrendsRequest : IRequest<AnalyseTrendsResults>
{
    public RunTradingStrategyResult StrategyResults { get; set; }

    public Dataset Dataset { get; set; }
}

public class AnalyseTrendsRequestHandler : IRequestHandler<AnalyseTrendsRequest, AnalyseTrendsResults>
{
    public async Task<AnalyseTrendsResults> Handle(AnalyseTrendsRequest request, CancellationToken cancellationToken)
    {
        await Task.Yield();
        var trends = request.StrategyResults.Actions.Trends(request.Dataset.Quotes).ToList();
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
        var datapoints = request.Dataset.Quotes.Values.First();

        return new AnalyseTrendsResults()
        {
            TradingResult = request.StrategyResults,
            AverageDownTrendDuration = TimeSpan.FromTicks((long)trends.Select(t => t.Bottom.Time - t.Start.Time).Where(t => t.TotalDays > 0).Select(t => t.Ticks).Average()),
            TrendsUnsorted = trends,
            TrendsByVolatility = trends.OrderBy(p => p.Volatility).ToList(),
            TrendsByDuration = trends.OrderByDescending(p => p.Duration).ToList(),
            WorstTrend = trends.MinBy(p => p.Volatility),
            BestTrend = trends.MaxBy(p => p.Volatility)
        };
    }    
}
