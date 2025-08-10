namespace TraderBot.Strategies;

public class MacdStrategyWithHedge : ITradingStrategy
{
    public Macd LongMacd { get; set; } = new Macd()
    {
        Fast = 2,
        Slow = 50,
        Signal = 30
    };

    public Macd ShortMacd { get; set; } = new Macd()
    {
        Fast = 2,
        Slow = 50,
        Signal = 30
    };

    public string LongSymbol { get; set; }

    public string ShortSymbol { get; set; }

    public Portfolio Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, Portfolio portfolio)
    {
        var longDataPoints = dataset[LongSymbol];
        var shortDataPointsByTime = dataset[ShortSymbol].ToDictionary(p => p.Time);

        TradingAction buyLong = null;
        TradingAction buyShort = null;

        var start = (double)longDataPoints[0].AdjustedClosingPrice;

        var yesterdayMacdLong = new Macd()
        {
            Fast = start,
            Slow = start,
            Signal = start
        };

        var yesterdayMacdShort = new Macd()
        {
            Fast = start,
            Slow = start,
            Signal = start
        };

        var blockShorts = false;

        foreach (var data in longDataPoints)
        {
            var macdLong = Indicators.Macd((double)data.AdjustedClosingPrice, LongMacd, yesterdayMacdLong);
            var macdShort = Indicators.Macd((double)data.AdjustedClosingPrice, ShortMacd, yesterdayMacdShort);

            if (buyShort != null && data.Time.Subtract(buyShort.Stock.Time) > System.TimeSpan.FromDays(2))
            {
                var shortData = shortDataPointsByTime[data.Time];
                portfolio = portfolio.Sell(buyShort.Quantity, shortData, macdShort);
                buyShort = null;
                blockShorts = true;
            }
            
            if (macdLong.Signal < macdLong.Value && yesterdayMacdLong.Signal > yesterdayMacdLong.Value)
            {
                if (buyShort != null)
                {
                    portfolio = portfolio.Sell(buyShort.Quantity, shortDataPointsByTime[data.Time], macdShort);
                    buyShort = null;
                }

                (portfolio, buyLong) = portfolio.Buy(portfolio.Usd, data, macdLong);
                blockShorts = false;
            }

            if (macdLong.Signal > macdLong.Value)
            {
                if (yesterdayMacdLong.Signal < yesterdayMacdLong.Value)
                {
                    if (buyLong != null)
                    {
                        portfolio = portfolio.Sell(buyLong.Quantity, data, macdLong);
                        buyLong = null;
                    }
                }
                
                if (shortDataPointsByTime.TryGetValue(data.Time, out var shortData) && buyShort == null && !blockShorts)
                {
                    (portfolio, buyShort) = portfolio.Buy(portfolio.Usd, shortData, macdLong);
                }
            }

            yesterdayMacdLong = macdLong;
            portfolio = portfolio.Updatevalue(dataset, data.Time);
        }

        if (buyLong != null)
        {
            var last = longDataPoints.Last();
            portfolio = portfolio.Sell(buyShort.Quantity, last, null);
        }

        if (buyShort != null)
        {
            var shortData = shortDataPointsByTime[longDataPoints.Last().Time];
            portfolio = portfolio.Sell(buyShort.Quantity, shortData, null);
        }

        return portfolio;
    }
}
