namespace TraderBot.Requests;

public class RunTradingStrategyRequest : IRequest<RunTradingStrategyResult>
{
    public Portfolio Portfolio { get; set; }

    public ITradingStrategy Strategy { get; set; } = new BuyAndHoldStrategy();

    public Dataset Dataset { get; set; }
}

public class RunTradingStrategyRequestHandler : IRequestHandler<RunTradingStrategyRequest, RunTradingStrategyResult>
{
    public async Task<RunTradingStrategyResult> Handle(RunTradingStrategyRequest request, CancellationToken cancellationToken)
    {
        await Task.Yield();
        var datapoints = request.Dataset.Quotes.Values.First();
        var portfolio = request.Strategy.Run(request.Dataset.Quotes, request.Portfolio);
        if (!portfolio.Actions.Any())
        {
            return new RunTradingStrategyResult()
            {
                Strategy = request.Strategy,
                Portfolio = portfolio,
            };
        }

        var sell = portfolio.Actions.Last();

        foreach (var action in portfolio.Actions)
        {
            action.Usd = Math.Round(action.Usd, 2);
        }

        var returnPercentage = (sell.Usd - request.Portfolio.Usd) / request.Portfolio.Usd * 100;

        var first = datapoints.First();
        var last = datapoints.Last();
        var totalYears = (last.Time - first.Time).TotalDays / 356;
        var yearlyReturnPercentage = (Math.Pow((double)sell.Usd / (double)request.Portfolio.Usd, 1 / totalYears) - 1) * 100;

        return new RunTradingStrategyResult()
        {
            Strategy = request.Strategy,
            TotalReturnPercentage = returnPercentage,
            YearlyReturnPercentage = (decimal)yearlyReturnPercentage,
            Portfolio = portfolio
        };
    }    
}
