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

        public static double Sma(this IEnumerable<double> source, double intervals)
        {
            return source.Sum() / intervals;
        }

        public static IEnumerable<double> Ema(this IEnumerable<double> source, int intervals) 
        {
            return source.SelectWithLastOut((source, last) => Ema(source, intervals, last));
        }

        public static IEnumerable<T> SelectWithLastOut<T>(this IEnumerable<T> source, Func<T,T,T> selector)
        {
            var last = source.FirstOrDefault();

            foreach (var item in source)
            {
                var value = selector(item, last);
                yield return value;
                last = value;
            }
        }

        public static IEnumerable<T> SelectWithLastIn<T>(this IEnumerable<T> source, Func<T, T, T> selector)
        {
            var last = source.FirstOrDefault();

            foreach (var item in source)
            {
                yield return selector(item, last);
                last = item;
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


        public static double Rsi(this IEnumerable<double> source, int currentElemenet, int interval)
        {
            var skip = Math.Max(0, currentElemenet - (interval + 1));
            var take = Math.Min(interval + 1, currentElemenet + 1);

            var diffs = source.Skip(skip).Take(take).SelectWithLastIn((current, last) => current - last);

            var positive = diffs.Select(v => v > 0 ? v : 0).Ema(interval).Last();
            var negative = diffs.Select(v => v < 0 ? v : 0).Ema(interval).Last();

            var rs = Math.Abs(positive / negative);
            var rsi = 100 - 100 / (1 + rs);

            return rsi;
        }
    }
}
