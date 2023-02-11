namespace TraderBot.Requests;

public class AddTimeSeriesRequest : IRequest<TimeSeries>
{
    public string SymbolName { get; set; }

    public AlphaVantage.Net.Common.Intervals.Interval Interval { get; set; }
}

public class AddTimeSeriesRequestHandler : IRequestHandler<AddTimeSeriesRequest, TimeSeries>
{
    private readonly TradingContext tradingContext;

    public AddTimeSeriesRequestHandler(TradingContext tradingContext)
    {
        this.tradingContext = tradingContext;
    }

    public async Task<TimeSeries> Handle(AddTimeSeriesRequest request, CancellationToken cancellationToken)
    {
        var symbol = await tradingContext.Symbols.Include(s => s.TimeSeries).FirstOrDefaultAsync(s => s.Name == request.SymbolName, cancellationToken);
        if (symbol == null)
        {
            symbol = new Symbol()
            {
                Name = request.SymbolName,
                TimeSeries = new List<TimeSeries>()
            };
            tradingContext.Symbols.Add(symbol);
        }

        var dailySeries = symbol.TimeSeries.FirstOrDefault(s => s.Interval == request.Interval);
        if (dailySeries == null)
        {
            dailySeries = new TimeSeries()
            {
                Symbol = symbol,
                Interval = request.Interval
            };
            symbol.TimeSeries.Add(dailySeries);
        }

        await tradingContext.SaveChangesAsync(cancellationToken);
        return dailySeries;
    }
}
