﻿namespace TraderBot.Models;

public class StockDataPoint
{
    [Key]
    public long Id { get; set; }

    public int TimeSeriesId { get; set; }

    public virtual TimeSeries TimeSeries { get; set; }

    public decimal ClosingPrice { get; set; }

    public decimal OpeningPrice { get; set; }

    public decimal LowestPrice { get; set; }

    public decimal HighestPrice { get; set; }

    public decimal AdjustedClosingPrice { get; set; }

    public DateTime Time { get; set; }

    public long Volume { get; set; }

    public override string ToString()
    {
        return $"{Time} AdjustedClosingPrice {AdjustedClosingPrice}";
    }
}
