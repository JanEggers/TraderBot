using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
