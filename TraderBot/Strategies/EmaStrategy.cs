namespace TraderBot.Strategies;

public class EmaStrategy : ITradingStrategy
{
    public double Ema { get; set; } = 200;

    public List<TradingAction> Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, decimal usd)
    {
        var stockDataPoints = dataset.Values.First();
        TradingAction buy = null;

        var result = new List<TradingAction>();

        var start = (double)stockDataPoints[0].AdjustedClosingPrice;

        var yesterdayEma = start;
        var yesterdayPrice = start;

        foreach (var data in stockDataPoints)
        {
            var price = (double)data.AdjustedClosingPrice;
            var ema = Indicators.Ema(price, Ema, yesterdayEma);
            if (price > ema && yesterdayEma > yesterdayPrice && usd > 0)
            {
                buy = new TradingAction()
                {
                    Usd = usd,
                    Op = TradingAction.Operation.OpenBuy,
                    Stock = data,
                    Quantity = usd / data.AdjustedClosingPrice,
                    Indicator = ema
                };
                result.Add(buy);
                usd = 0;
            }

            if (ema < price && yesterdayEma < yesterdayPrice && buy != null)
            {
                usd = buy.Quantity * data.AdjustedClosingPrice;
                var sell = new TradingAction()
                {
                    Usd = usd,
                    Op = TradingAction.Operation.CloseBuy,
                    Stock = data,
                    Quantity = buy.Quantity,
                    Diff = usd - buy.Usd,
                    Indicator = ema
                };
                result.Add(sell);
                buy = null;
            }

            yesterdayEma = ema; 
            yesterdayPrice = price;
        }

        if (buy != null)
        {
            var last = stockDataPoints.Last();
            usd = buy.Quantity * last.AdjustedClosingPrice;
            var sell = new TradingAction()
            {
                Usd = usd,
                Op = TradingAction.Operation.CloseBuy,
                Stock = last,
                Quantity = buy.Quantity
            };
            result.Add(sell);
        }

        return result;
    }
}
