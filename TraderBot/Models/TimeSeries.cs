namespace TraderBot.Models;

public class TimeSeries
{

    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Symbol))]
    public int SymbolId { get; set; }
    public Symbol Symbol { get; set; }

    public ICollection<StockDataPoint> DataPoints { get; set; }
}
