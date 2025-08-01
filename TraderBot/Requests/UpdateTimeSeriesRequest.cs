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
        
        var lastUpdate = await tradingContext.StockDataPoints
            .Where(p => p.TimeSeriesId == request.TimeSeries.Id)
            .OrderByDescending(p => p.Time)
            .FirstOrDefaultAsync(cancellationToken);

        var newPoints = series.AsEnumerable();

        if (lastUpdate != null)
        {
            newPoints = newPoints.Where(p => p.Timestamp > lastUpdate.Time);
        }

        newPoints = newPoints.OrderBy(p => p.Timestamp);

        foreach (var item in newPoints)
        {
            var dp = new StockDataPoint()
            {
                TimeSeriesId = request.TimeSeries.Id,
                Time = item.Timestamp,
                OpeningPrice = item.Open ?? (decimal)0.0,
                ClosingPrice = item.Close ?? (decimal)0.0,
                HighestPrice = item.High ?? (decimal)0.0,
                LowestPrice = item.Low ?? (decimal)0.0,
                AdjustedClosingPrice = item.Close ?? (decimal)0.0,
                Volume = item.Volume ?? 0
            };

            tradingContext.Add(dp);
        }

        await tradingContext.SaveChangesAsync(cancellationToken);

        lastUpdate = await tradingContext.StockDataPoints.OrderByDescending(p => p.Time).FirstOrDefaultAsync(cancellationToken);

        logger.LogInformation($"updated {request.TimeSeries.Symbol.Name} until {lastUpdate.Time}");
    }
}
