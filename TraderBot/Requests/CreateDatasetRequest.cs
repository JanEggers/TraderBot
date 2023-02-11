using System.Data;

namespace TraderBot.Requests;

public class CreateDatasetRequest : IRequest<Dataset>
{
    public List<string> Symbols { get; set; }

    public AlphaVantage.Net.Common.Intervals.Interval Interval { get; set; }

    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}

public class CreateDatasetRequestHanlder : IRequestHandler<CreateDatasetRequest, Dataset>
{
    private readonly TradingContext _context;

    public CreateDatasetRequestHanlder(TradingContext context)
    {
        _context = context;
    }

    public async Task<Dataset> Handle(CreateDatasetRequest request, CancellationToken cancellationToken)
    {
        var result = new Dataset();

        foreach (var symbol in request.Symbols)
        {
            var series = await _context.TimeSeries
                .Include(p => p.Symbol)
                .FirstOrDefaultAsync(p => p.Symbol.Name == symbol && p.Interval == request.Interval, cancellationToken);

            if (series == null)
            {
                throw new InvalidOperationException($"no series was found for symbol {symbol} with interval {request.Interval}");
            }

            var data = _context.StockDataPoints
                .Where(p => p.TimeSeriesId == series.Id);

            if (request.Start.HasValue)
            {
                data = data.Where(p => p.Time >= request.Start.Value);
            }

            if (request.End.HasValue)
            {
                data = data.Where(p => p.Time >= request.End.Value);
            }

            var datapoints = await data
                .OrderBy(p => p.Time)
                .ToListAsync(cancellationToken);

            result.Quotes.Add(symbol, datapoints);
            result.Start = datapoints.First().Time;
            result.End = datapoints.Last().Time;
        }

        return result;        
    }
}