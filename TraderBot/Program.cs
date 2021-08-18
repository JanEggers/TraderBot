using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TraderBot.Requests;
using TraderBot.Extensions;
using Microsoft.AspNetCore.Hosting;

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

            await host.RunAsync();

            //var timeSeries = await host.Send(new AddTimeSeriesRequest()
            //{
            //    SymbolName = "AAPL",
            //    Interval = AlphaVantage.Net.Common.Intervals.Interval.Daily
            //});

            //await host.Send(new UpdateTimeSeriesRequest() { TimeSeries = timeSeries });
        }
    }
}
