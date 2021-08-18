﻿using AlphaVantage.Net.Stocks.Client;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TraderBot.Models;

namespace TraderBot.Requests
{
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
        public async Task<Unit> Handle(UpdateTimeSeriesRequest request, CancellationToken cancellationToken)
        {
            var series = await stocksClient.GetTimeSeriesAsync(request.TimeSeries.Symbol.Name,
                request.TimeSeries.Interval,
                AlphaVantage.Net.Common.Size.OutputSize.Full, isAdjusted: true);
            
            var lastUpdate = await tradingContext.StockDataPoints
                .Where(p => p.TimeSeriesId == request.TimeSeries.Id)
                .OrderByDescending(p => p.Time)
                .FirstOrDefaultAsync(cancellationToken);

            var newPoints = series.DataPoints.AsEnumerable().OfType<AlphaVantage.Net.Stocks.StockAdjustedDataPoint>();

            if (lastUpdate != null)
            {
                newPoints = newPoints.Where(p => p.Time > lastUpdate.Time);
            }

            newPoints = newPoints.OrderBy(p => p.Time);

            foreach (var item in newPoints)
            {
                tradingContext.Add(new StockDataPoint()
                {
                    TimeSeriesId = request.TimeSeries.Id,
                    Time = item.Time,
                    OpeningPrice = item.OpeningPrice,
                    ClosingPrice = item.ClosingPrice,
                    HighestPrice = item.HighestPrice,
                    LowestPrice = item.LowestPrice,
                    AdjustedClosingPrice = item.AdjustedClosingPrice,
                    Volume = item.Volume
                });
            }

            await tradingContext.SaveChangesAsync(cancellationToken);

            lastUpdate = await tradingContext.StockDataPoints.OrderByDescending(p => p.Time).FirstOrDefaultAsync(cancellationToken);

            logger.LogInformation($"updated {request.TimeSeries.Symbol.Name} until {lastUpdate.Time}");

            return Unit.Value;
        }
    }
}
