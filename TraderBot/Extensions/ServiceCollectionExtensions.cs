using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MediatR;
using TraderBot.Models;
using AlphaVantage.Net.Core.Client;
using AlphaVantage.Net.Stocks.Client;
using Microsoft.Data.Sqlite;
using System.IO;

namespace TraderBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTraderBot(this IServiceCollection services, string apiKey)
        {
            services.AddDbContext<TradingContext>(ctx => 
            {
                var db = new SqliteConnectionStringBuilder();

                var path = Path.GetFullPath("Data.db");

                db.DataSource = path;
                ctx.UseSqlite(db.ConnectionString);
            });
            services.AddMediatR(typeof(Program).Assembly);

            services.AddScoped((c) => new AlphaVantageClient(apiKey));
            services.AddScoped((c) => c.GetRequiredService<AlphaVantageClient>().Stocks());

            return services;
        }
    }
}
