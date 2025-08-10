namespace TraderBot.Models;

public record PortfolioValue
{
    public DateTime Timestamp { get; init; }
    public decimal Usd { get; init; }
}
