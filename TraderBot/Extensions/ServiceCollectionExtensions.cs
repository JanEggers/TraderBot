using AlphaVantage.Net.Core.Client;
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
        services.AddMediatR(typeof(TradingContext).Assembly);

        services.AddScoped((c) => new AlphaVantageClient(apiKey));
        services.AddScoped((c) => c.GetRequiredService<AlphaVantageClient>().Stocks());

        return services;
    }
}
