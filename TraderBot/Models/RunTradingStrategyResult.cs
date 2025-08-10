namespace TraderBot.Models;

public class RunTradingStrategyResult
{
    public decimal TotalReturnPercentage { get; set; }
    public decimal YearlyReturnPercentage { get; set; }

    public ITradingStrategy Strategy { get; set; }

    public Portfolio Portfolio { get; set; }
}
