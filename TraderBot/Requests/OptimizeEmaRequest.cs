using System.Collections.Concurrent;

namespace TraderBot.Requests;

public class OptimizeEmaRequest : IRequest
{
    public Dataset Dataset { get; set; }
}

public class OptimizeEmaRequestHandler : IRequestHandler<OptimizeEmaRequest>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OptimizeMacdRequestHandler> _logger;

    public OptimizeEmaRequestHandler(
        IServiceScopeFactory serviceScopeFactory, 
        ILogger<OptimizeMacdRequestHandler> logger) 
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Handle(OptimizeEmaRequest request, CancellationToken cancellationToken)
    {
        RunTradingStrategyResult best = null;
        double bestema = 0;
        var max = 200;

        var strategies = Enumerable.Range(1, max).Select(k => new EmaStrategy()
        {
            Ema = k
        });

        var results = new ConcurrentBag<RunTradingStrategyResult>();

        await Parallel.ForEachAsync(strategies, async (strategy, ct) =>
        {
            var macd = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
            {
                Portfolio = new Portfolio() { Usd = 1000 },
                Strategy = strategy,
                Dataset = request.Dataset
            });
            results.Add(macd);
        });

        foreach (var macd in results)
        {
            if (best == null || best.Portfolio.Usd < macd.Portfolio.Usd)
            {
                if (macd.Portfolio.Actions.Count > 0)
                {
                    bestema = ((EmaStrategy)macd.Strategy).Ema;
                    best = macd;
                    _logger.LogInformation($"new best {bestema} {best.Portfolio.Usd} {best.YearlyReturnPercentage}");
                }
            }
        }
    }
}
