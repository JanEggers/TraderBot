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
            "NASDX",    //buyandhold    5.3%/-83% // macd 18/4/2   
            "AAPL",     //buyandhold   26%/-80% // macd 2/61/98  
            "TQQQ",     //buyandhold   35%/-81% // macd 23/30/3   with hedge 45%/-16%
            "SWDA.LON", //buyandhold   11,4%/-26% // macd 5/3/2    
            "UVXY",     //buyandhold -100% // macd 61/19/99  -15%
            "SQQQ",     //buyandhold  -54% // macd 66/46/94 -3,1%
            "DTE.DEX"   //buyandhold    6,3%/-40% // macd 19/3/81 9.6%/-31%
        };

        var symbol = 3;
        var shortS = -1;

        var macds = new Macd[]
        {
            new () // 11,6%/-19%
            {
                Fast = 18,
                Slow = 4,
                Signal = 2
            },
            new () // 23%/-18%
            {
                Fast = 2,
                Slow = 61,
                Signal = 98
            },
            new () //  35%/-23%
            {
                Fast = 23,
                Slow = 30,
                Signal = 3
            },
            new () // 9,3%/-13%
            {
                Fast = 5,
                Slow = 3,
                Signal = 2
            },
            new () // -15%
            {
                Fast = 19,
                Slow = 3,
                Signal = 81
            },
            new () // -3,1%
            {
                Fast = 66,
                Slow = 46,
                Signal = 94
            }
        };

        var emas = new double[]
        {
            197,  // 7,45%  / -14%
            137, // 24,9%  / -19%
            200,  // 26,9% / -30%
            164,  // 6,1%  / -9,4%
            200, // -3,6%  / -66%
            123, // -17%  / -30%
            2   // 3,1%  / -3,4%
        };

        var longSymbol = symbols[symbol];
        var shortSymbol = shortS >=0 ? symbols[shortS] : string.Empty;
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
            Symbols = new List<string> { longSymbol, shortSymbol },
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

        //var macd = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        //{
        //    Usd = 1000,
        //    Strategy = new MacdStrategy()
        //    {
        //        Macd = macds[symbol]
        //    },
        //    Dataset = dataset
        //});

        //var macdTrends = await _serviceScopeFactory.Send(new AnalyseTrendsRequest()
        //{
        //    StrategyResults = macd,
        //    Dataset = dataset
        //});

        var ema = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        {
            Usd = 1000,
            Strategy = new EmaStrategy() {
                Ema = emas[symbol]
                //Ema = 200
            },
            Dataset = dataset
        });

        var emaTrends = await _serviceScopeFactory.Send(new AnalyseTrendsRequest()
        {
            StrategyResults = ema,
            Dataset = dataset
        });

        //var macdhedged = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
        //{
        //    Usd = 1000,
        //    Strategy = new MacdStrategyWithHedge()
        //    {
        //        LongMacd = macds[symbol],
        //        ShortMacd = macds[shortS],
        //        LongSymbol = longSymbol,
        //        ShortSymbol= shortSymbol,
        //    },
        //    Dataset = dataset
        //});

        //await _serviceScopeFactory.Send(new OptimizeEmaRequest()
        //{
        //    Dataset = dataset
        //});

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
