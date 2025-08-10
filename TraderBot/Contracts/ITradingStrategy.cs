namespace TraderBot.Contracts;

public interface ITradingStrategy
{
    Portfolio Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, Portfolio portfolio);
}
