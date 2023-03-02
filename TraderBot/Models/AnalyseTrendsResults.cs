namespace TraderBot.Models;

public class AnalyseTrendsResults 
{
    public RunTradingStrategyResult TradingResult { get; set; }

    public List<Trend> TrendsByVolatility { get; set; }
    public List<Trend> TrendsByDuration { get; set; }
    public List<Trend> TrendsUnsorted { get; set; }

    public Trend WorstTrend { get; set; }

    public Trend BestTrend { get; set; }

    public TimeSpan AverageDownTrendDuration { get; set; }
}
