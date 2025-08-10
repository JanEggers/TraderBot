using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TraderBot.Strategies;

public class EmaStrategy : ITradingStrategy
{
    public double Ema { get; set; } = 200;

    public Portfolio Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, Portfolio portfolio)
    {
        var stockDataPoints = dataset.Values.First();
        TradingAction buy = null;

        var start = (double)stockDataPoints[0].AdjustedClosingPrice;

        var yesterdayEma = start;
        var yesterdayPrice = start;

        foreach (var data in stockDataPoints)
        {
            var price = (double)data.AdjustedClosingPrice;
            var ema = Indicators.Ema(price, Ema, yesterdayEma);
            if (price > ema && yesterdayEma > yesterdayPrice && portfolio.Usd > 0)
            {
                (portfolio, buy) = portfolio.Buy(portfolio.Usd, data, ema);
            }

            if (price < ema && buy != null)
            {
                portfolio = portfolio.Sell(buy.Quantity, data, ema);
                buy = null;
            }

            yesterdayEma = ema; 
            yesterdayPrice = price;
        }

        if (buy != null)
        {
            var last = stockDataPoints.Last();
            portfolio = portfolio.Sell(buy.Quantity, last, null);
        }

        return portfolio;
    }
}
