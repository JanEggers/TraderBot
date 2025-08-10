namespace TraderBot.Strategies;

public class MacdStrategy : ITradingStrategy
{
    public Macd Macd { get; set; } = new Macd()
    {
        Fast = 2,
        Slow = 50,
        Signal = 30
    };

    public Portfolio Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, Portfolio portfolio)
    {
        var stockDataPoints = dataset.Values.First();
        TradingAction buy = null;

        var start = (double)stockDataPoints[0].AdjustedClosingPrice;

        var yesterdayMacd = new Macd()
        {
            Fast = start,
            Slow = start,
            Signal = start
        };

        foreach (var data in stockDataPoints)
        {
            var macd = Indicators.Macd((double)data.AdjustedClosingPrice, Macd, yesterdayMacd);
            if (macd.Signal < macd.Value && yesterdayMacd.Signal > yesterdayMacd.Value)
            {
                (portfolio, buy) = portfolio.Buy(portfolio.Usd, data, macd);
            }

            if (macd.Signal > macd.Value && yesterdayMacd.Signal < yesterdayMacd.Value && buy != null)
            {
                portfolio = portfolio.Sell(buy.Quantity, data, macd);
                buy = null;
            }

            yesterdayMacd = macd;
        }

        if (buy != null)
        {
            var last = stockDataPoints.Last();
            portfolio = portfolio.Sell(buy.Quantity, last, null);
        }

        return portfolio;
    }
}
