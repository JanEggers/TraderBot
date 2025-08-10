namespace TraderBot.Strategies;

public class BuyAndHoldStrategy : ITradingStrategy
{
    public Portfolio Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, Portfolio portfolio)
    {
        var datapoints = dataset.Values.First();
        var first = datapoints.First();
        var last = datapoints.Last();

        (portfolio, var buy) = portfolio.Buy(portfolio.Usd, first, null);

        portfolio = portfolio.Sell(buy.Quantity, last, null);

        return portfolio;
    }
}
