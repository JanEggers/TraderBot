using System.Collections.Generic;

namespace TraderBot.Models
{
    public class RunTradingStrategyResult
    {
        public decimal Usd { get; set; }

        public decimal TotalReturnPercentage { get; set; }
        public decimal YearlyReturnPercentage { get; set; }

        public List<TradingAction> Actions { get; set; } = new List<TradingAction>();
    }
}
