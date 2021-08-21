using System.Collections.Generic;
using TraderBot.Models;

namespace TraderBot.Contracts
{
    public interface ITradingStrategy
    {
        List<TradingAction> Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, decimal usd);
    }
}
