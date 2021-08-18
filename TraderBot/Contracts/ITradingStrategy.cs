using System.Collections.Generic;
using TraderBot.Models;

namespace TraderBot.Contracts
{
    public interface ITradingStrategy
    {
        List<TradingAction> Run(List<StockDataPoint> stockDataPoints, decimal usd);
    }
}
