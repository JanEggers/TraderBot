namespace TraderBot.Models;

public class Dataset
{
    public Dictionary<string, IReadOnlyList<StockDataPoint>> Quotes { get; set; } = new();

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
