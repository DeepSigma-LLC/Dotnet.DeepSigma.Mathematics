using System.Data;
using DeepSigma.General.DateTimeUnification;
using DeepSigma.General.Enums;
using DeepSigma.General.Extensions;

namespace DeepSigma.Mathematics.Statistics;

/// <summary>
/// Statistical utility functions for financial calculations.
/// </summary>
public static class StatisticsUtilities
{
    /// <summary>
    /// The coefficient of variation is a measure of relative variability that is calculated as the ratio of the standard deviation to the mean.
    /// </summary>
    /// <param name="standard_deviation">The standard deviation of the data set.</param>
    /// <param name="mean">The mean of the data set.</param>
    /// <returns>The coefficient of variation.</returns>
    public static decimal ComputeCoefficientOfVariation(decimal standard_deviation, decimal mean) => mean == 0 ? decimal.MaxValue : standard_deviation / mean; // Avoid division by zero, implies infinite coefficient of variation


    /// <summary>
    /// Standard Error calculates the standard error of an estimate based on the estimate value and the sample size.
    /// </summary>
    /// <param name="estimate">The estimate value for which the standard error is being calculated.</param>
    /// <param name="sample_size">The size of the sample used to calculate the estimate.</param>
    /// <returns>The calculated standard error for the given estimate and sample size.</returns>
    public static decimal StandardError(decimal estimate, double sample_size) => estimate / (decimal)Math.Sqrt(sample_size);

    /// <summary>
    /// EstimateStandardDeviationFromRange provides an estimate of the standard deviation of a process based on the range defined by the upper and lower limits.
    /// This is not mathematically rigorous, but can be a useful heuristic when you have limited data and want to get a rough sense of the variability in the process.
    /// </summary>
    /// <param name="upper_limit">The upper limit of the range.</param>
    /// <param name="lower_limit">The lower limit of the range.</param>
    /// <param name="divisor">The divisor used to estimate the standard deviation from the range. Default is 4 for small sample sizes such as less than 20.
    /// For larger sample sizes, a divisor of 6 is often used to provide a more accurate estimate of the standard deviation based on the range. 
    /// The choice of divisor can depend on the specific context and the desired level of precision in the estimate. The divisor comes from the fact that for a normal distribution, approximately 95% of the data falls within 2 standard deviations of the mean, which corresponds to a range of 4 standard deviations (2 on each side of the mean). 
    /// For larger sample sizes, using a divisor of 6 can provide a more accurate estimate of the standard deviation based on the range, as it accounts for the increased variability that can occur with larger samples.
    /// </param>
    /// <returns>The estimated standard deviation based on the range.</returns>
    public static decimal EstimateStandardDeviationFromRange(decimal upper_limit, decimal lower_limit, decimal divisor = 4) => (upper_limit - lower_limit) / divisor;

    /// <summary>
    /// Calculates the cumulative distribution function (CDF) for a normal distribution given the mean, standard deviation, and a specific value x.
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="std"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal CalculateCDF(decimal mean, decimal std, decimal x)
    {
        return (decimal)MathNet.Numerics.Distributions.Normal.CDF((double)mean, (double)std, (double)x);
    }


    /// <summary>
    /// Calculates the inverse of the normal cumulative distribution function (CDF) for a given probability p, mean, and standard deviation.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="mean"></param>
    /// <param name="std"></param>
    /// <returns></returns>
    public static decimal CalculateNormInverse(decimal p, decimal mean, decimal std)
    {
        return (decimal)MathNet.Numerics.Distributions.Normal.InvCDF((double)mean, (double)std, (double)p);
    }


    /// <summary>
    /// Calculates the probability density function (PDF) for a normal distribution given the mean, standard deviation, and a specific value x.
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="std"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static decimal CalculatePDF(decimal mean, decimal std, decimal x)
    {
        return (decimal)MathNet.Numerics.Distributions.Normal.PDF((double)mean, (double)std, (double)x);
    }

    /// <summary>
    /// Calculates the Z-Score for a given value, mean, and standard deviation.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="mean"></param>
    /// <param name="std"></param>
    /// <returns></returns>
    public static decimal CalculateZScore(decimal value, decimal mean, decimal std) => (value - mean) / std;

    /// <summary>
    /// Caluclates the total return of a return series.
    /// </summary>
    /// <param name="returns"></param>
    /// <returns></returns>
    public static decimal CalculateTotalReturn(decimal[] returns)
    {
        return returns.Aggregate(1m, (accumulation, observationReturn) => accumulation * (1 + observationReturn)) - 1;
    }

    /// <summary>
    /// Calculates the combined standard deviation of two correlated variables based on their individual standard
    /// deviations and the correlation coefficient.
    /// </summary>
    /// <param name="std_1">The standard deviation of the first variable. Must be a non-negative value.</param>
    /// <param name="std_2">The standard deviation of the second variable. Must be a non-negative value.</param>
    /// <param name="correlation">The correlation coefficient between the two variables. Must be between -1 and 1, inclusive.</param>
    /// <returns>The standard deviation of the combined variables, accounting for their correlation, as a decimal value.</returns>
    public static decimal CalculateStandardDeviationOfCorrelatedVariables(decimal std_1, decimal std_2, decimal correlation)
    {
        decimal variance_1 = Math.Pow(std_1, 2);
        decimal variance_2 = Math.Pow(std_2, 2);
        return Math.Sqrt(variance_1 + variance_2 + 2 * correlation * std_1 * std_2 );
    }

    /// <summary>
    ///  Caluclates the total return of a return series.
    /// </summary>
    /// <param name="Data"></param>
    /// <returns></returns>
    public static decimal CalculateAnnualizedReturn<TDate>(SortedDictionary<TDate, decimal> Data)
        where TDate : struct, IDateTime<TDate>
    {
        decimal totalReturn = CalculateTotalReturn(Data.Values.ToArray());
        Periodicity periodicity = PeriodicityUtilities.GetEstimatedPeriodicityUsingFuzzyLogic(Data.Keys.Select(x => x.DateTime).ToArray());
        int PeriodsPerYear = PeriodicityUtilities.GetPeriodsPerYear(periodicity);
        TimeSpan timeSpan = Data.Keys.Max() - Data.Keys.Min();
        double DaysPerPeriod = 365.25 / PeriodsPerYear;
        double PeriodsWithinSeries = timeSpan.TotalDays / DaysPerPeriod;
        decimal AnnualizationFactor = PeriodsPerYear.ToDecimal() / PeriodsWithinSeries.ToDecimal();
        return Math.Pow((1 + totalReturn), AnnualizationFactor) - 1;
    }

    /// <summary>
    /// Measures the volatility of a security/portfolio relative to the market.
    /// </summary>
    /// <param name="portfolioReturns"></param>
    /// <param name="marketReturns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static decimal CalculateBeta(decimal[] portfolioReturns, decimal[] marketReturns)
    {
        if (portfolioReturns.Length != marketReturns.Length)
            throw new ArgumentException("Return streams must have the same length.");

        decimal covariance = CalculateCovariance(portfolioReturns, marketReturns);
        decimal varianceMarket = CalculateSampleVariance(marketReturns);
        return covariance / varianceMarket;
    }

    /// <summary>
    /// Calculates the variance of a data set, which quantifies how much the values deviate from the mean. A higher variance indicates greater spread, while a lower variance suggests that the values are closer to the mean. 
    /// </summary>
    /// <param name="dataset"></param>
    /// <returns></returns>
    public static decimal CalculateSampleVariance(decimal[] dataset)
    {
        decimal mean = dataset.Average();
        return dataset.Select(x => Math.Pow((x - mean), 2)).Sum() / (dataset.Length - 1);
    }

    /// <summary>
    /// Calculates the standard deviation of a data set, which measures the amount of variation or dispersion from the mean. A higher standard deviation indicates greater spread in the data, while a lower value suggests the data points are closer to the mean. 
    /// </summary>
    /// <param name="dataset"></param>
    /// <returns></returns>
    public static decimal CalculateSampleStandardDeviation(decimal[] dataset)
    {
        return Math.Sqrt(CalculateSampleVariance(dataset));
    }

    /// <summary>
    /// Calculates the standard deviation of a data set, which measures the amount of variation or dispersion from the mean. A higher standard deviation indicates greater spread in the data, while a lower value suggests the data points are closer to the mean. 
    /// </summary>
    /// <param name="Data"></param>
    /// <returns></returns>
    public static decimal CalculateAnnulizedVolatility<TDate>(SortedDictionary<TDate, decimal> Data) 
        where TDate : struct, IDateTime<TDate>
    {
        decimal StandardDeviation = Math.Sqrt(CalculateSampleVariance(Data.Values.ToArray()));
        decimal annualizationMultiplier = PeriodicityUtilities.GetAnnualizationMultiplier(Data.Keys.Select(x => x.DateTime).ToArray());
        return StandardDeviation * annualizationMultiplier;
    }

    /// <summary>
    /// Calculates the strength and direction of two data sets relationship. It returns a correlation coefficient ranging from -1 (perfect negative correlation) to +1 (perfect positive correlation), with 0 indicating no correlation. Often represented as the variable R in mathematics. Range (-1, 1).
    /// </summary>
    /// <param name="dataset1"></param>
    /// <param name="dataset2"></param>
    /// <returns></returns>
    public static decimal CalculateCorrelation(decimal[] dataset1, decimal[] dataset2)
    {
        decimal covariance = CalculateCovariance(dataset1, dataset2);
        decimal stdDev1 = CalculateSampleStandardDeviation(dataset1);
        decimal stdDev2 = CalculateSampleStandardDeviation(dataset2);

        return covariance / (stdDev1 * stdDev2);
    }

    /// <summary>
    /// Calculates the R-squared (coefficient of determination) value, which measures how well one data set explains the variance in another. R-squared ranges from 0 to 1, where 0 indicates no explanatory power and 1 indicates a perfect fit.
    /// </summary>
    /// <param name="portfolioReturns"></param>
    /// <param name="marketReturns"></param>
    /// <returns></returns>
    public static decimal CalculateRSquared(decimal[] portfolioReturns, decimal[] marketReturns)
    {
        decimal correlation = CalculateCorrelation(portfolioReturns, marketReturns);
        return correlation.PowerExact(2);
    }

    /// <summary>
    /// Measures the abnormal return of a portfolio relative to what the CAPM model predicts.
    /// </summary>
    /// <param name="portfolioReturn"></param>
    /// <param name="marketReturn"></param>
    /// <param name="riskFreeRate"></param>
    /// <param name="beta"></param>
    /// <returns></returns>
    public static decimal CalculateJensensAlpha(decimal portfolioReturn, decimal marketReturn, decimal riskFreeRate, decimal beta)
    {
        return portfolioReturn - (riskFreeRate + beta * (marketReturn - riskFreeRate));
    }

    /// <summary>
    /// Calulates the covariance of two return streams. Aka: Quantification of how two return streams co-vary/move together. Indicates the direction of the linear relationship between variables. Range: (-inf, inf).
    /// </summary>
    /// <param name="portfolioReturns"></param>
    /// <param name="marketReturns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static decimal CalculateCovariance(decimal[] portfolioReturns, decimal[] marketReturns)
    {
        if (portfolioReturns.Length != marketReturns.Length) throw new ArgumentException("Return streams must have the same length.");

        int observationCount = portfolioReturns.Length;
        decimal avgPortfolioReturn = portfolioReturns.Average();
        decimal avgMarketReturn = marketReturns.Average();

        decimal covariance = portfolioReturns.Zip(marketReturns, (portfolioReturn, marketReturn) => (portfolioReturn - avgPortfolioReturn) * (marketReturn - avgMarketReturn)).Sum() / (observationCount - 1);
        return covariance;
    }

    /// <summary>
    /// Calculates the annulized tracking error of a portfolio relative to a benchmark. 
    /// Tracking error measures the volatility of the difference between the portfolio returns and the benchmark returns. 
    /// A lower tracking error indicates that the portfolio closely follows the benchmark, while a higher tracking error suggests greater deviation.
    /// </summary>
    /// <param name="portfolioReturns"></param>
    /// <param name="marketReturns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static decimal CalculateAnnulizedTrackingError<TDate>(SortedDictionary<TDate, decimal> portfolioReturns, SortedDictionary<TDate, decimal> marketReturns)
        where TDate : struct, IDateTime<TDate>
    {
        if (portfolioReturns.Count != marketReturns.Count)  throw new ArgumentException("Return streams must have the same length.");

        decimal annulizedTrackingError = CalculateAnnulizedVolatility(CalculateExcessReturnSeries(portfolioReturns, marketReturns));
        return annulizedTrackingError;
    }

    /// <summary>
    /// Calculates the excess return series by subtracting the market returns from the portfolio returns for each corresponding time period.
    /// </summary>
    /// <param name="portfolioReturns"></param>
    /// <param name="marketReturns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static SortedDictionary<TDate, decimal> CalculateExcessReturnSeries<TDate>(SortedDictionary<TDate, decimal> portfolioReturns, SortedDictionary<TDate, decimal> marketReturns)
        where TDate : struct, IDateTime<TDate>
    {
        if (portfolioReturns.Count != marketReturns.Count) throw new ArgumentException("Return streams must have the same length.");

        List<decimal> excessReturns = portfolioReturns.Zip(marketReturns, (portfolioReturn, marketReturn) => portfolioReturn.Value - marketReturn.Value).ToList();
        SortedDictionary<TDate, decimal> results = [];
        foreach (TDate dateTime in portfolioReturns.Keys)
        {
            int returnIndex = portfolioReturns.Keys.ToList().IndexOf(dateTime);
            results.Add(dateTime, excessReturns[returnIndex]);
        }
        return results;
    }


    /// <summary>
    /// Calculates the max drawdown from a return series.
    /// </summary>
    /// <param name="returns"></param>
    /// <returns></returns>
    public static decimal CalculateMaxDrawdown(decimal[] returns)
    {
        decimal peak = 0;
        decimal maxDrawdown = 0;
        decimal cumulativeValue = 1; // Starts at an initial value of 1

        foreach (var returnValue in returns)
        {
            cumulativeValue *= 1 + returnValue; // Update the cumulative value with the return
            peak = Math.Max(peak, cumulativeValue); // Update the peak if we have a new higher peak
            decimal drawdown = (peak - cumulativeValue) / peak; // Calculate drawdown from peak
            maxDrawdown = Math.Max(maxDrawdown, drawdown); // Update max drawdown if current drawdown is larger
        }
        return maxDrawdown;
    }
}
