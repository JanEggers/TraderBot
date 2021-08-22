using System;
using System.Collections.Generic;
using System.Linq;
using TraderBot.Models;

namespace TraderBot.Extensions
{
    public static class Indicators
    {
        public static double Ema(double source, double intervals, double lastEma)
        {
            double k = 2 / (intervals + 1);
            return source * k + lastEma * (1 - k);
        }

        public static IEnumerable<double> Ema(this IEnumerable<double> source, int intervals) 
        {
            return source.SelectWithLast((source, last) => Ema(source, intervals, last));
        }

        public static IEnumerable<T> SelectWithLast<T>(this IEnumerable<T> source, Func<T,T,T> selector)
        {
            var last = source.FirstOrDefault();

            foreach (var item in source)
            {
                var value = selector(item, last);
                yield return value;
                last = value;
            }
        }

        public static Macd Macd(double source, Macd intervals, Macd last)
        {
            var slowema = Ema(source, intervals.Slow, last.Slow);
            var fastema = Ema(source, intervals.Fast, last.Fast);

            var macd = fastema - slowema;

            var signalema = Ema(macd, intervals.Signal, last.Signal);

            return new Macd()
            {
                Slow = slowema,
                Fast = fastema,
                Value = macd,
                Signal = signalema
            };
        }

        public static IEnumerable<(TradingAction, TradingAction)> Trades(this IEnumerable<TradingAction> source)
        {
            foreach (var item in source.Page(2))
            {
                yield return (item[0], item[1]);
            }
        }

        public static IEnumerable<IReadOnlyList<T>> Page<T>(this IEnumerable<T> source, int size)
        {
            var page = new List<T>(size);

            foreach (var item in source)
            {
                page.Add(item);
                if (page.Count == size)
                {
                    yield return page;
                    page = new List<T>(size);
                }
            }
        }

        public static IEnumerable<Trend> Trends(this IEnumerable<TradingAction> source, Dictionary<string, IReadOnlyList<StockDataPoint>> dataset)
        {
            foreach (var (buyAction, sellAction) in source.Trades())
            {
                var relevantPoints = dataset[buyAction.Stock.TimeSeries.Symbol.Name].SkipWhile(p => buyAction.Stock != p).TakeWhile(p => sellAction.Stock != p);

                var peak = relevantPoints.First();
                var bottom = peak;
                var start = peak;

                foreach (var item in relevantPoints)
                {
                    if (item.AdjustedClosingPrice > peak.AdjustedClosingPrice)
                    {
                        peak = item;

                        yield return new Trend()
                        {
                            Start = start,
                            Peak = peak,
                            Bottom = bottom,
                        };
                        bottom = peak;
                        start = peak;
                    }

                    if (item.AdjustedClosingPrice < bottom.AdjustedClosingPrice)
                    {
                        bottom = item;
                    }
                }
            }
        }
    }
}
