namespace TraderBot.Requests;

public class UpdateTimeSeriesRequest : IRequest
{
    public TimeSeries TimeSeries { get; set; }
}

public class UpdateTimeSeriesRequestHandler : IRequestHandler<UpdateTimeSeriesRequest>
{
    private readonly TradingContext tradingContext;
    private readonly StocksClient stocksClient;
    private readonly ILogger<UpdateTimeSeriesRequestHandler> logger;

    public UpdateTimeSeriesRequestHandler(TradingContext tradingContext, StocksClient stocksClient, ILogger<UpdateTimeSeriesRequestHandler> logger)
    {
        this.tradingContext = tradingContext;
        this.stocksClient = stocksClient;
        this.logger = logger;
    }
    public async Task Handle(UpdateTimeSeriesRequest request, CancellationToken cancellationToken)
    {
        var series = await stocksClient.GetTimeSeriesAsync(request.TimeSeries.Symbol.Name,
            request.TimeSeries.Interval,
            AlphaVantage.Net.Common.Size.OutputSize.Full, isAdjusted: true);
        
        var lastUpdate = await tradingContext.StockDataPoints
            .Where(p => p.TimeSeriesId == request.TimeSeries.Id)
            .OrderByDescending(p => p.Time)
            .FirstOrDefaultAsync(cancellationToken);

        var newPoints = series.DataPoints.AsEnumerable();

        if (lastUpdate != null)
        {
            newPoints = newPoints.Where(p => p.Time > lastUpdate.Time);
        }

        newPoints = newPoints.OrderBy(p => p.Time);

        foreach (var item in newPoints)
        {
            var dp = new StockDataPoint()
            {
                TimeSeriesId = request.TimeSeries.Id,
                Time = item.Time,
                OpeningPrice = item.OpeningPrice,
                ClosingPrice = item.ClosingPrice,
                HighestPrice = item.HighestPrice,
                LowestPrice = item.LowestPrice,
                AdjustedClosingPrice = item.ClosingPrice,
                Volume = item.Volume
            };

            switch (item)
            {
                case AlphaVantage.Net.Stocks.StockAdjustedDataPoint adjusted:
                    dp.AdjustedClosingPrice = adjusted.AdjustedClosingPrice;
                    break;
                default:
                    break;
            }

            tradingContext.Add(dp);
        }

        await tradingContext.SaveChangesAsync(cancellationToken);

        lastUpdate = await tradingContext.StockDataPoints.OrderByDescending(p => p.Time).FirstOrDefaultAsync(cancellationToken);

        logger.LogInformation($"updated {request.TimeSeries.Symbol.Name} until {lastUpdate.Time}");
    }
}
