namespace TraderBot.Models;
public class TradingAction
{
    public enum Operation
    {
        OpenBuy,
        CloseBuy
    }

    public Operation Op { get; set; }

    public decimal Usd { get; set; }

    public decimal Diff { get; set; }

    public StockDataPoint Stock { get; set; }

    public decimal Quantity { get; set; }

    public override string ToString()
    {
        switch (Op)
        {
            case Operation.CloseBuy:
                return $"{Op} {Stock.TimeSeries.Symbol.Name} {Usd} {Stock.Time} {Diff}";
            case Operation.OpenBuy:
            default:
                return $"{Op} {Stock.TimeSeries.Symbol.Name} {Usd} {Stock.Time}";
        }
    }
}
