using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TraderBot.Requests;
using TraderBot.Extensions;
using Microsoft.AspNetCore.Hosting;
using AlphaVantage.Net.Stocks.Client;
using Microsoft.Extensions.DependencyInjection;
using TraderBot.Strategies;

namespace TraderBot
{
    class Program
    {

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(web => 
            {
                web.UseStartup<Startup>();
            });

        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            await host.Send(new MigrateDatabaseRequest());

            var symbols = new[] { "NASDX", "AAPL", "TQQQ", "SWDA.LON" };

            var symbol = symbols[2];
            var interval = AlphaVantage.Net.Common.Intervals.Interval.Min60;

            //await host.RunAsync();

            //var timeSeries = await host.Send(new AddTimeSeriesRequest()
            //{
            //    SymbolName = symbol,
            //    Interval = interval
            //});

            //await host.Send(new UpdateTimeSeriesRequest() { TimeSeries = timeSeries });


            //var result = await host.Services.CreateScope().ServiceProvider.GetRequiredService<StocksClient>().SearchSymbolAsync("swda.l");

            var buyAndHold = await host.Send(new RunTradingStrategyRequest()
            {
                SymbolName = symbol,
                Usd = 1000,
                Strategy = new BuyAndHoldStrategy(),
                Interval = interval,
            });

            var macd = await host.Send(new RunTradingStrategyRequest()
            {
                SymbolName = symbol,
                Usd = 1000,
                Strategy = new MacdStrategy() 
                {
                    Macd = new Macd() 
                    {
                        Fast = 2,
                        Slow = 50,
                        Signal = 30
                    }
                },
                Interval = interval,                
            });
        }
    }
}
