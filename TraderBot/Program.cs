using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TraderBot.Requests;
using TraderBot.Extensions;
using Microsoft.AspNetCore.Hosting;
using AlphaVantage.Net.Stocks.Client;
using Microsoft.Extensions.DependencyInjection;

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

            var symbols = new[] { "NASDX", "AAPL" };

            //await host.RunAsync();

            //var timeSeries = await host.Send(new AddTimeSeriesRequest()
            //{
            //    SymbolName = "NASDX",
            //    Interval = AlphaVantage.Net.Common.Intervals.Interval.Daily
            //});

            //await host.Send(new UpdateTimeSeriesRequest() { TimeSeries = timeSeries });


            //var result = await host.Services.CreateScope().ServiceProvider.GetRequiredService<StocksClient>().SearchSymbolAsync("nasdaq");

            var result = await host.Send(new RunTradingStrategyRequest()
            {
                SymbolName = "NASDX",
                Usd = 1000
            });
        }
    }
}
