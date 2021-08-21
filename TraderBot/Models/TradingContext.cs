using Microsoft.EntityFrameworkCore;

namespace TraderBot.Models
{
    public class TradingContext : DbContext
    {
        public TradingContext(DbContextOptions<TradingContext> options)
            : base(options)
        {
        }

        public DbSet<Symbol> Symbols { get; set; }
        public DbSet<TimeSeries> TimeSeries { get; set; }
        public DbSet<StockDataPoint> StockDataPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StockDataPoint>().HasIndex(p => new
            {
                p.TimeSeriesId,
                p.Time
            }).IsUnique();


            modelBuilder.Entity<TimeSeries>().Property(p => p.Interval).HasConversion<string>();

            modelBuilder.Entity<TimeSeries>().HasIndex(p => new
            {
                p.SymbolId,
                p.Interval
            }).IsUnique();

            modelBuilder.Entity<Symbol>().HasIndex(p => new
            {
                p.Name,
            }).IsUnique();
        }
    }
}
