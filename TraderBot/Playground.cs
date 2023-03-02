using MediatR;
using System.Collections.Concurrent;
using System.Threading;

namespace TraderBot;

public class Playground : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<Playground> _logger;

    public Playground(IServiceScopeFactory serviceScopeFactory, ILogger<Playground> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _serviceScopeFactory.Send(new MigrateDatabaseRequest());

        var symbols = new[] {
            "NASDX",    //buyandhold    5.3%/-83% // macd 18/4/2   11,6%/-19%
            "AAPL",     //buyandhold   26%/-80% // macd 2/61/98  23%/-18%
            "TQQQ",     //buyandhold   35%/-81% // macd 23/30/3  35%/-23%  with hedge 45%/-16%
            "SWDA.LON", //buyandhold   11,4%/-26% // macd 5/3/2    9,3%/-13%
            "UVXY",     //buyandhold -100% // macd 61/19/99  -15%
            "SQQQ",     //buyandhold  -54% // macd 66/46/94 -3,1%
            "DTE.DEX"   //buyandhold    6,3%/-40% // macd 19/3/81 9.6%/-31%
        };

        var symbol = 3;

        var macds = new Macd[]
        {
            new ()
            {
                Fast = 18,
                Slow = 4,
                Signal = 2
            },
            new ()
            {
                Fast = 2,
                Slow = 61,
                Signal = 98
            },
            new ()
            {
                Fast = 23,
                Slow = 30,
                Signal = 3
            },
            new ()
            {
                Fast = 5,
                Slow = 3,
                Signal = 2
            },
            new ()
            {
                Fast = 19,
                Slow = 3,
                Signal = 81
            }
        };

        var longSymbol = symbols[symbol];
        var shortSymbol = symbols[3];
        var interval = AlphaVantage.Net.Common.Intervals.Interval.Daily;

        //var timeSeries = await _serviceScopeFactory.Send(new AddTimeSeriesRequest()
        //{
        //    SymbolName = longSymbol,
        //    Interval = interval
        //});

        //await _serviceScopeFactory.Send(new UpdateTimeSeriesRequest() { TimeSeries = timeSeries });


        //var result = await _serviceScopeFactory.Services.CreateScope().ServiceProvider.GetRequiredService<StocksClient>().SearchSymbolAsync("DTE");

        var dataset = await _serviceScopeFactory.Send(new CreateDatasetRequest()
        {
            Symbols = new List<string> { longSymbol },
            Interval = interval,
        });

        var buyAndHold = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        {
            Usd = 1000,
            Strategy = new BuyAndHoldStrategy(),
            Dataset = dataset
        });

        var buyAndHoldTrends = await _serviceScopeFactory.Send(new AnalyseTrendsRequest()
        {
            StrategyResults = buyAndHold,
            Dataset = dataset
        });

        var macd = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        {
            Usd = 1000,
            Strategy = new MacdStrategy()
            {
                Macd = macds[symbol]
            },
            Dataset = dataset
        });

        var macdTrends = await _serviceScopeFactory.Send(new AnalyseTrendsRequest()
        {
            StrategyResults = macd,
            Dataset = dataset
        });


        await _serviceScopeFactory.Send(new OptimizeMacdRequest() 
        {
            Dataset = dataset
        });


        //var macdWithHedge = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        //{
        //    SymbolNames = new string[] { longSymbol, shortSymbol },
        //    Usd = 1000,
        //    Strategy = new MacdStrategyWithHedge()
        //    {
        //        LongMacd = new Macd()
        //        {
        //            Fast = 4,
        //            Slow = 37,
        //            Signal = 11
        //        },
        //        ShortMacd = new Macd()
        //        {
        //            Fast = 2,
        //            Slow = 36,
        //            Signal = 26
        //        },
        //        LongSymbol = longSymbol,
        //        ShortSymbol = shortSymbol
        //    },
        //    Interval = interval,
        //});
    }
}
