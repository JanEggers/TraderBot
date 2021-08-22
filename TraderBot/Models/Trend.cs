namespace TraderBot.Models
{
    public class Trend
    {
        public StockDataPoint Start { get; set; }
        public StockDataPoint Peak { get; set; }

        public StockDataPoint Bottom { get; set; }

        public override string ToString()
        {
            return $"{Start.Time} {Peak.Time} {Bottom.AdjustedClosingPrice} {Peak.AdjustedClosingPrice}";
        }
    }
}
