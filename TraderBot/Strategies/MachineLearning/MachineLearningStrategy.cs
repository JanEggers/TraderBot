using Microsoft.ML;

namespace TraderBot.Strategies.MachineLearning;

public class MachineLearningStrategy : ITradingStrategy
{
    public Portfolio Run(IReadOnlyDictionary<string, IReadOnlyList<StockDataPoint>> dataset, Portfolio portfolio)
    {
        var stockDataPoints = dataset.Values.First();

        var mlContext = new MLContext();

        var dataView = mlContext.Data.LoadFromEnumerable(stockDataPoints
            .Where(p => p.Time.Year < 2012)
            .Select(p => new ModelInput() 
        {
            Price = (double)p.AdjustedClosingPrice,
            Time = p.Time,
            Year = p.Time.Year
        }));

        //https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/time-series-demand-forecasting

        var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
            outputColumnName: "Op",
            inputColumnName: "Price",
            windowSize: 7,
            seriesLength: 30,
            trainSize: 365,
            horizon: 7,
            confidenceLevel: 0.95f);

        var forecaster = forecastingPipeline.Fit(dataView);

        //IDataView predictions = model.Transform(testData);

        return portfolio;
    }
}
