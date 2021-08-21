using System.Collections.Generic;
using System.Linq;
using TraderBot.Contracts;
using TraderBot.Extensions;
using TraderBot.Models;

namespace TraderBot.Strategies
{
    public class MacdStrategy : ITradingStrategy
    {
        public Macd Macd { get; set; } = new Macd()
        {
            Fast = 2,
            Slow = 50,
            Signal = 30
        };

        public List<TradingAction> Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, decimal usd)
        {
            var stockDataPoints = dataset.Values.First();
            TradingAction buy = null;

            var result = new List<TradingAction>();

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
                    buy = new TradingAction()
                    {
                        Usd = usd,
                        Op = TradingAction.Operation.OpenBuy,
                        Stock = data,
                        Quantity = usd / data.AdjustedClosingPrice
                    };
                    result.Add(buy);
                }

                if (macd.Signal > macd.Value && yesterdayMacd.Signal < yesterdayMacd.Value && buy != null)
                {
                    usd = buy.Quantity * data.AdjustedClosingPrice;
                    var sell = new TradingAction()
                    {
                        Usd = usd,
                        Op = TradingAction.Operation.CloseBuy,
                        Stock = data,
                        Quantity = buy.Quantity,
                        Diff = usd - buy.Usd
                    };
                    result.Add(sell);
                    buy = null;
                }

                yesterdayMacd = macd;
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
}
