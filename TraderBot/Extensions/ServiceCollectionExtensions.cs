using Microsoft.Data.Sqlite;
using System.IO;

namespace TraderBot.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTraderBot(this IServiceCollection services, string apiKey)
    {
        services.AddHostedService<Playground>();
        services.AddDbContext<TradingContext>(ctx => 
        {
            var db = new SqliteConnectionStringBuilder();

            var path = Path.GetFullPath("Data.db");

            db.DataSource = path;
            ctx.UseSqlite(db.ConnectionString);
            ctx.EnableSensitiveDataLogging();
        });
        services.AddMediatR(mediatr => {
            mediatr.RegisterServicesFromAssembly(typeof(TradingContext).Assembly);
        });

        services.AddAlphaVantageClient(alphaVantage => 
        {
            alphaVantage.ApiKey = apiKey;
            alphaVantage.MaxApiCallsPerMinute = 100;
        });

        return services;
    }
}
