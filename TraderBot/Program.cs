using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TraderBot.Requests;
using TraderBot.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using AlphaVantage.Net.Stocks.Client;
using Microsoft.Extensions.DependencyInjection;
using TraderBot.Strategies;
using TraderBot.Models;

namespace TraderBot
{
    class Program
    {

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(web =>
            {
                web.UseStartup<Startup>();
            });

        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            await host.Send(new MigrateDatabaseRequest());
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            var symbols = new[] {
                "NASDX",    //buyandhold    6% // macd 4/37/11   3%
                "AAPL",     //buyandhold   27% // macd 4/37/11  17%
                "TQQQ",     //buyandhold   53% // macd 4/37/11  44%  with hedge 45%
                "SWDA.LON", //buyandhold   12% // macd 2/8/36    6%
                "UVXY",     //buyandhold -100% // macd 2/3/60  -50%
                "SQQQ",     //buyandhold  -54% // macd 2/36/26 -29%
                "DTE.DEX"   //buyandhold    6% // macd 4/37/11 1.6%
            };

            var longSymbol = symbols[2];
            var shortSymbol = symbols[5];
            var interval = AlphaVantage.Net.Common.Intervals.Interval.Daily;

            //await host.RunAsync();

            //var timeSeries = await host.Send(new AddTimeSeriesRequest()
            //{
            //    SymbolName = longSymbol,
            //    Interval = interval
            //});

            //await host.Send(new UpdateTimeSeriesRequest() { TimeSeries = timeSeries });


            //var result = await host.Services.CreateScope().ServiceProvider.GetRequiredService<StocksClient>().SearchSymbolAsync("DTE");

            var buyAndHold = await host.Send(new RunTradingStrategyRequest()
            {
                SymbolNames = new string[] { longSymbol },
                Usd = 1000,
                Strategy = new BuyAndHoldStrategy(),
                Interval = interval,
            });

            var macd = await host.Send(new RunTradingStrategyRequest()
            {
                SymbolNames = new string[] { longSymbol },
                Usd = 1000,
                Strategy = new MacdStrategy()
                {
                    Macd = new Macd()
                    {
                        Fast = 4,
                        Slow = 37,
                        Signal = 11
                    }
                },
                Interval = interval,
            });

            //RunTradingStrategyResult best = null;
            //Macd bestmacd = null;
            //var max = 100;

            //for (int i = 2; i < max; i++)
            //{
            //    for (var j = 3; j < max; j++)
            //    {
            //        for (int k = 2; k < max; k++)
            //        {
            //            var strategy = new MacdStrategy()
            //            {
            //                Macd = new Macd()
            //                {
            //                    Fast = i,
            //                    Slow = j,
            //                    Signal = k
            //                }
            //            };
            //            var macd = await host.Send(new RunTradingStrategyRequest()
            //            {
            //                SymbolNames = new string[] { longSymbol },
            //                Usd = 1000,
            //                Strategy = strategy,
            //                Interval = interval,
            //            });

            //            //logger.LogInformation($"Try Macd {strategy.Macd}");

            //            if (best == null || best.Usd < macd.Usd)
            //            {
            //                if (macd.Actions.Count > 0)
            //                {
            //                    bestmacd = strategy.Macd;
            //                    best = macd;
            //                    logger.LogInformation($"new best {bestmacd} {best.Usd} {best.YearlyReturnPercentage}");
            //                }
            //            }
            //        }
            //    }
            //}



            var macdWithHedge = await host.Send(new RunTradingStrategyRequest()
            {
                SymbolNames = new string[] { longSymbol, shortSymbol },
                Usd = 1000,
                Strategy = new MacdStrategyWithHedge()
                {
                    LongMacd = new Macd()
                    {
                        Fast = 4,
                        Slow = 37,
                        Signal = 11
                    },
                    ShortMacd = new Macd() 
                    {
                        Fast = 2,
                        Slow = 36,
                        Signal = 26
                    },
                    LongSymbol = longSymbol,
                    ShortSymbol = shortSymbol
                },
                Interval = interval,
            });
        }
    }
}
