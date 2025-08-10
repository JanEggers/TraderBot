using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Text.Json;

namespace TraderBot.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public string ClosingPrice { get; set; } = string.Empty;
    public string PortfolioValueBuyAndHold { get; set; } = string.Empty;
    public string PortfolioValueMacd { get; set; } = string.Empty;

    public IndexModel(ILogger<IndexModel> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task OnGet()
    {
        var symbol = "SWDA.LON";
        var dataset = await _serviceScopeFactory.Send(new CreateDatasetRequest()
        {
            Symbols = new List<string> { symbol },
        });

        var points = dataset.Quotes[symbol].Select(a =>
        {
            return new DataPoint(DateOnly.FromDateTime(a.Time), a.AdjustedClosingPrice);
        }).ToList();

        this.ClosingPrice = JsonSerializer.Serialize(points, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));



        var buyAndHold = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        {
            Portfolio = new Portfolio() { Usd = 1000 },
            Strategy = new BuyAndHoldStrategy(),
            Dataset = dataset
        });

        points = buyAndHold.Portfolio.Value.Select(a =>
        {
            return new DataPoint(DateOnly.FromDateTime(a.Timestamp), a.Usd);
        }).ToList();
        this.PortfolioValueBuyAndHold = JsonSerializer.Serialize(points, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));



        var macd = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        {
            Portfolio = new Portfolio() { Usd = 1000 },
            Strategy = new MacdStrategy()
            {
                Macd = new()
                {
                    Fast = 5,
                    Slow = 3,
                    Signal = 2
                }
            },
            Dataset = dataset
        });


        points = macd.Portfolio.Value.Select(a =>
        {
            return new DataPoint(DateOnly.FromDateTime(a.Timestamp), a.Usd);
        }).ToList();
        this.PortfolioValueMacd = JsonSerializer.Serialize(points, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
