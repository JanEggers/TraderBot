using System;
using System.Collections.Generic;
using System.Linq;
using TraderBot.Contracts;
using TraderBot.Models;

namespace TraderBot.Strategies
{
    public class BuyAndHoldStrategy : ITradingStrategy
    {
        public List<TradingAction> Run(List<StockDataPoint> datapoints, decimal usd)
        {
            var first = datapoints.First();
            var last = datapoints.Last();
            var buy = new TradingAction()
            {
                Usd = usd,
                Op = TradingAction.Operation.OpenBuy,
                Stock = first,
                Quantity = usd / first.AdjustedClosingPrice
            };

            var sell = new TradingAction()
            {
                Usd = buy.Quantity * last.AdjustedClosingPrice,
                Op = TradingAction.Operation.CloseBuy,
                Stock = last,
                Quantity = buy.Quantity
            };

            return new List<TradingAction>()
            {
                buy, 
                sell
            };
        }
    }
}
