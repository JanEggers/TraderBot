using System.Collections.Generic;
using System.Linq;
using TraderBot.Contracts;
using TraderBot.Models;

namespace TraderBot.Strategies
{
    public class MacdStrategy : ITradingStrategy
    {
        public int Fast { get; set; } = 2;

        public int Slow { get; set; } = 50;

        public int Signal { get; set; } = 30;

        public List<TradingAction> Run(List<StockDataPoint> stockDataPoints, decimal usd)
        {
            var slowema = (double)stockDataPoints[0].AdjustedClosingPrice;
            var fastema = (double)stockDataPoints[0].AdjustedClosingPrice;
            var signalema = (double)stockDataPoints[0].AdjustedClosingPrice;
            var yesterdaySlowEma = slowema;
            var yesterdayFastEma = fastema;
            var yesterdaySignalema = signalema;
            var yesterdayMacd = signalema;
            TradingAction buy = null;

            var result = new List<TradingAction>();

            foreach (var data in stockDataPoints)
            {
                slowema = Ema((double)data.AdjustedClosingPrice, Slow, yesterdaySlowEma);
                fastema = Ema((double)data.AdjustedClosingPrice, Fast, yesterdayFastEma);

                var macd = fastema - slowema;

                signalema = Ema(macd, Signal, yesterdaySignalema);

                if (signalema < macd && yesterdaySignalema > yesterdayMacd)
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

                if (signalema > macd && yesterdaySignalema < yesterdayMacd && buy != null)
                {
                    usd = buy.Quantity * data.AdjustedClosingPrice;
                    var sell = new TradingAction()
                    {
                        Usd = usd,
                        Op = TradingAction.Operation.CloseBuy,
                        Stock = data,
                        Quantity = buy.Quantity
                    };
                    result.Add(sell);
                    buy = null;
                }

                yesterdaySlowEma = slowema;
                yesterdayFastEma = fastema;
                yesterdaySignalema = signalema;
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

        private static double Ema(double todaysPrice, double numberOfDays, double EMAYesterday) 
        {
            double k = 2 / (numberOfDays + 1);
            return todaysPrice * k + EMAYesterday * (1 - k);
        }
    }
}
