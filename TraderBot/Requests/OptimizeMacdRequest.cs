using System.Collections.Concurrent;

namespace TraderBot.Requests;

public class OptimizeMacdRequest : IRequest
{
    public Dataset Dataset { get; set; }
}

public class OptimizeMacdRequestHandler : IRequestHandler<OptimizeMacdRequest>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OptimizeMacdRequestHandler> _logger;

    public OptimizeMacdRequestHandler(
        IServiceScopeFactory serviceScopeFactory, 
        ILogger<OptimizeMacdRequestHandler> logger) 
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Handle(OptimizeMacdRequest request, CancellationToken cancellationToken)
    {
        RunTradingStrategyResult best = null;
        Macd bestmacd = null;
        var max = 100;

        for (int i = 2; i < max; i++)
        {
            for (var j = 3; j < max; j++)
            {
                var strategies = Enumerable.Range(2, 98).Select(k => new MacdStrategy()
                {
                    Macd = new Macd()
                    {
                        Fast = i,
                        Slow = j,
                        Signal = k
                    }
                });

                var sem = new SemaphoreSlim(1);

                await Parallel.ForEachAsync(strategies, new ParallelOptions() { MaxDegreeOfParallelism = 16 }, async (strategy, ct) =>
                {
                    var macd = await _serviceScopeFactory.Send(new RunTradingStrategyRequest()
                    {
                        Portfolio = new Portfolio() { Usd = 1000 },
                        Strategy = strategy,
                        Dataset = request.Dataset
                    });

                    try
                    {
                        await sem.WaitAsync(cancellationToken);


                        if (best == null || best.Portfolio.Usd < macd.Portfolio.Usd)
                        {
                            if (macd.Portfolio.Actions.Count > 0)
                            {
                                bestmacd = ((MacdStrategy)macd.Strategy).Macd;
                                best = macd;
                                _logger.LogInformation($"new best {bestmacd} {best.Portfolio.Usd} {best.YearlyReturnPercentage}");
                            }
                        }
                    }
                    finally
                    {
                        sem.Release();
                    }
                });
            }
        }
    }
}
