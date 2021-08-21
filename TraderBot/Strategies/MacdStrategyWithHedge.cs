using System.Collections.Generic;
using System.Linq;
using TraderBot.Contracts;
using TraderBot.Extensions;
using TraderBot.Models;

namespace TraderBot.Strategies
{
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

        public List<TradingAction> Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, decimal usd)
        {
            var longDataPoints = dataset[LongSymbol];
            var shortDataPointsByTime = dataset[ShortSymbol].ToDictionary(p => p.Time);

            TradingAction buyLong = null;
            TradingAction buyShort = null;

            var result = new List<TradingAction>();

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
                    usd = buyShort.Quantity * shortData.AdjustedClosingPrice;
                    var sell = new TradingAction()
                    {
                        Usd = usd,
                        Op = TradingAction.Operation.CloseBuy,
                        Stock = shortData,
                        Quantity = buyShort.Quantity,
                        Diff = usd - buyShort.Usd
                    };
                    result.Add(sell);
                    buyShort = null;
                    blockShorts = true;
                }
                
                if (macdLong.Signal < macdLong.Value && yesterdayMacdLong.Signal > yesterdayMacdLong.Value)
                {
                    if (buyShort != null)
                    {
                        var shortData = shortDataPointsByTime[data.Time];
                        usd = buyShort.Quantity * shortData.AdjustedClosingPrice;
                        var sell = new TradingAction()
                        {
                            Usd = usd,
                            Op = TradingAction.Operation.CloseBuy,
                            Stock = shortData,
                            Quantity = buyShort.Quantity,
                            Diff = usd - buyShort.Usd
                        };
                        result.Add(sell);
                        buyShort = null;
                    }

                    buyLong = new TradingAction()
                    {
                        Usd = usd,
                        Op = TradingAction.Operation.OpenBuy,
                        Stock = data,
                        Quantity = usd / data.AdjustedClosingPrice
                    };
                    result.Add(buyLong);
                    blockShorts = false;
                }

                if (macdLong.Signal > macdLong.Value)
                {
                    if (yesterdayMacdLong.Signal < yesterdayMacdLong.Value)
                    {
                        if (buyLong != null)
                        {
                            usd = buyLong.Quantity * data.AdjustedClosingPrice;
                            var sell = new TradingAction()
                            {
                                Usd = usd,
                                Op = TradingAction.Operation.CloseBuy,
                                Stock = data,
                                Quantity = buyLong.Quantity,
                                Diff = usd - buyLong.Usd
                            };
                            result.Add(sell);
                            buyLong = null;
                        }
                    }
                    
                    if (shortDataPointsByTime.TryGetValue(data.Time, out var shortData) && buyShort == null && !blockShorts)
                    {
                        buyShort = new TradingAction()
                        {
                            Usd = usd,
                            Op = TradingAction.Operation.OpenBuy,
                            Stock = shortData,
                            Quantity = usd / shortData.AdjustedClosingPrice
                        };

                        result.Add(buyShort);
                    }
                }

                yesterdayMacdLong = macdLong;
            }

            if (buyLong != null)
            {
                var last = longDataPoints.Last();
                usd = buyLong.Quantity * last.AdjustedClosingPrice;
                var sell = new TradingAction()
                {
                    Usd = usd,
                    Op = TradingAction.Operation.CloseBuy,
                    Stock = last,
                    Quantity = buyLong.Quantity
                };
                result.Add(sell);
            }

            if (buyShort != null)
            {
                var shortData = shortDataPointsByTime[longDataPoints.Last().Time];
                usd = buyShort.Quantity * shortData.AdjustedClosingPrice;
                var sell = new TradingAction()
                {
                    Usd = usd,
                    Op = TradingAction.Operation.CloseBuy,
                    Stock = shortData,
                    Quantity = buyShort.Quantity,
                    Diff = usd - buyShort.Usd
                };
                result.Add(sell);
            }

            return result;
        }
    }
}
