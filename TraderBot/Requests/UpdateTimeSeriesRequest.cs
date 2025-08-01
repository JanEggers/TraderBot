using TraderBot.Models;

namespace TraderBot.Requests;

public class UpdateTimeSeriesRequest : IRequest
{
    public TimeSeries TimeSeries { get; set; }
}

public class UpdateTimeSeriesRequestHandler : IRequestHandler<UpdateTimeSeriesRequest>
{
    private readonly TradingContext tradingContext;
    private readonly AlphaVantageClient stocksClient;
    private readonly ILogger<UpdateTimeSeriesRequestHandler> logger;

    public UpdateTimeSeriesRequestHandler(TradingContext tradingContext, AlphaVantageClient stocksClient, ILogger<UpdateTimeSeriesRequestHandler> logger)
    {
        this.tradingContext = tradingContext;
        this.stocksClient = stocksClient;
        this.logger = logger;
    }
    public async Task Handle(UpdateTimeSeriesRequest request, CancellationToken cancellationToken)
    {
        var series = await stocksClient.GetDailyTimeSeries(new DailyTimeSeriesRequest()
        {
            Symbol = request.TimeSeries.Symbol.Name,
            OutputSize = TimeSeriesOutputSize.Full,
        }, cancellationToken);


        tradingContext.StockDataPoints.RemoveRange(tradingContext.StockDataPoints
            .Where(p => p.TimeSeriesId == request.TimeSeries.Id));

        var lastCLose = 0.0;
        var multiplier = 1;


        foreach (var item in series.OrderBy(s => s.Timestamp))
        {
            var close = (double?)(item.Close) ?? 0.0;

            var compare = (lastCLose / 1.9);

            if (lastCLose > 0 && close < compare)
            {
                multiplier *= 2;
            }

            lastCLose = close;

            var dp = new StockDataPoint()
            {
                TimeSeriesId = request.TimeSeries.Id,
                Time = item.Timestamp,
                OpeningPrice = item.Open ?? (decimal)0.0,
                ClosingPrice = (decimal)close,
                HighestPrice = item.High ?? (decimal)0.0,
                LowestPrice = item.Low ?? (decimal)0.0,
                AdjustedClosingPrice = (decimal)close * multiplier,
                Volume = item.Volume ?? 0
            };

            tradingContext.Add(dp);
        }

        await tradingContext.SaveChangesAsync(cancellationToken);

        var lastUpdate = await tradingContext.StockDataPoints.OrderByDescending(p => p.Time).FirstOrDefaultAsync(cancellationToken);

        logger.LogInformation($"updated {request.TimeSeries.Symbol.Name} until {lastUpdate.Time}");
    }
}
