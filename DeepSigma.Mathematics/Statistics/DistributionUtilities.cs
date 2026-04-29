namespace DeepSigma.Mathematics.Statistics;

/// <summary>
/// Provides static utility methods for computing statistical distribution functions.
/// </summary>
public class DistributionUtilities
{
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
    /// Calculates the cumulative distribution function (CDF) for a log-normal distribution given the mean, standard deviation, and a specific value x.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="mean"></param>
    /// <param name="std"></param>
    /// <returns></returns>
    public static decimal CalculateLogNormCDF(decimal x, decimal mean, decimal std)
    {
        return (decimal)MathNet.Numerics.Distributions.LogNormal.CDF((double)mean, (double)std, (double)x);
    }

    /// <summary>
    /// Calculates the cumulative distribution function (CDF) of the Poisson distribution for a specified value and rate
    /// parameter.
    /// </summary>
    /// <remarks>This method uses the MathNet.Numerics library to compute the CDF. Ensure that <paramref
    /// name="x"/> and <paramref name="lambda"/> are within valid ranges to avoid unexpected results.</remarks>
    /// <param name="x">The value at which to evaluate the cumulative probability. Must be a non-negative integer.</param>
    /// <param name="lambda">The average rate (λ) of occurrences for the Poisson distribution. Must be a positive value.</param>
    /// <returns>A decimal value representing the probability that a Poisson-distributed random variable is less than or equal to
    /// <paramref name="x"/>.</returns>
    public static decimal CalculatePoissonCDF(decimal x, decimal lambda)
    {
        return (decimal)MathNet.Numerics.Distributions.Poisson.CDF((double)lambda, (double)x);
    }

    /// <summary>
    /// Log normal inverse CDF, also known as the quantile function, calculates the value x such that the cumulative distribution function (CDF) of a log-normal distribution with a given mean and standard deviation equals a specified probability p.
    /// In other words, it finds the value x for which P(X ≤ x) = p, where X is a log-normally distributed random variable. 
    /// This function is useful for determining thresholds or percentiles in a log-normal distribution based on a desired confidence level or probability.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="mean"></param>
    /// <param name="std"></param>
    /// <returns></returns>
    public static decimal CalculateLogNormInverse(decimal p, decimal mean, decimal std)
    {
        return (decimal)MathNet.Numerics.Distributions.LogNormal.CDF((double)mean, (double)std, (double)p);
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
}
