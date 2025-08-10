using System.Collections.Immutable;

namespace TraderBot.Models;

public record Portfolio
{
    public decimal Usd { get; init; }

    public ImmutableList<TradingAction> Actions { get; init; } = ImmutableList<TradingAction>.Empty;
    public ImmutableList<PortfolioValue> Value { get; init; } = ImmutableList<PortfolioValue>.Empty;

    public ImmutableDictionary<string, decimal> Stocks { get; init; } = ImmutableDictionary<string, decimal>.Empty;



    public (Portfolio, TradingAction) Buy(decimal usd, StockDataPoint data, object indicator) {
        var buy = new TradingAction()
        {
            Usd = usd,
            Op = TradingAction.Operation.OpenBuy,
            Stock = data,
            Quantity = usd / data.AdjustedClosingPrice,
            Indicator = indicator
        };

        return (this with
        {
            Usd = Usd - usd,
            Actions = Actions.Add(buy),
            Stocks = Stocks.SetItem(data.TimeSeries.Symbol.Name, Stocks.GetValueOrDefault(data.TimeSeries.Symbol.Name) + buy.Quantity)
        }, buy);
    }

    public Portfolio Sell(decimal quantity, StockDataPoint data, object indicator) {
        var usd = quantity * data.AdjustedClosingPrice;
        var sell = new TradingAction()
        {
            Usd = usd,
            Op = TradingAction.Operation.CloseBuy,
            Stock = data,
            Quantity = quantity,
            Diff = usd - Actions.Last(a => a.Stock.TimeSeries.SymbolId == data.TimeSeries.SymbolId).Usd,
            Indicator = indicator
        }; 
        
        return this with
        {
            Usd = Usd + usd,
            Actions = Actions.Add(sell),
            Stocks = Stocks.SetItem(data.TimeSeries.Symbol.Name, Stocks.GetValueOrDefault(data.TimeSeries.Symbol.Name) - quantity)
        };
    }

    public Portfolio Updatevalue(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, DateTime timestamp) 
    {
        var value = Usd;

        value += Stocks.Sum(s =>
        {
            var point = dataset[s.Key].First(p => p.Time == timestamp);
            return point.AdjustedClosingPrice * s.Value;
        });

        var lastPeak = Value.LastOrDefault()?.LastPeak ?? (decimal)0.0;

        var isPeak = value > lastPeak;


        return this with
        {
            Value = Value.Add(new PortfolioValue() 
            {
                Usd = value, 
                Timestamp = timestamp,
                Volatility = isPeak ? 1 : value / lastPeak,
                LastPeak = isPeak ? value : lastPeak
            })
        };
    }

}
