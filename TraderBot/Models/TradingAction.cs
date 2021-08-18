namespace TraderBot.Models
{
    public class TradingAction
    {
        public enum Operation
        {
            OpenBuy,
            CloseBuy
        }

        public Operation Op { get; set; }

        public decimal Usd { get; set; }

        public StockDataPoint Stock { get; set; }

        public decimal Quantity { get; set; }
    }
}
